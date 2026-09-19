using System.ComponentModel.DataAnnotations;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("timeon-score")]
[Route("api/public/timeon-score")]
public class TimeOnScoreController(
    ApplicationDbContext db,
    AccountBusinessLineService businessLineService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TimeOnScoreSubmitResponse>> Submit(TimeOnScoreSubmitRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return Ok(new TimeOnScoreSubmitResponse { Accepted = true });
        }

        if (!IsValidScore(request.OverallScore)
            || !IsValidScore(request.RegistrationScore)
            || !IsValidScore(request.IncidentsScore)
            || !IsValidScore(request.AdministrationScore)
            || !IsValidScore(request.TraceabilityScore)
            || !IsValidScore(request.ExperienceScore))
        {
            return BadRequest("Las puntuaciones deben estar entre 0 y 100.");
        }

        var companyName = request.Company.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;
        var (employeeMin, employeeMax) = ParseTeamSize(request.TeamSize);
        var priority = CalculatePriority(request.OverallScore, employeeMin, request.WantsContact);

        var account = await db.Accounts
            .FirstOrDefaultAsync(a =>
                a.AccountType == AccountType.Company
                && a.CompanyName != null
                && a.CompanyName == companyName);

        if (account is null)
        {
            account = new Account
            {
                AccountType = AccountType.Company,
                CompanyName = companyName,
                Email = email,
                Phone = Clean(request.Phone),
                EmployeeCountMin = employeeMin,
                EmployeeCountMax = employeeMax,
                LeadSource = LeadSource.Website,
                LeadPriority = priority,
                CurrentTimeTrackingSystem = Clean(request.CurrentSystem),
                IcpScore = request.OverallScore,
                PainHypothesis = BuildPainHypothesis(request),
                Notes = BuildAssessmentNote(request, now),
                CreatedAt = now,
                UpdatedAt = now,
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();
        }
        else
        {
            account.Email ??= email;
            account.Phone ??= Clean(request.Phone);
            account.EmployeeCountMin ??= employeeMin;
            account.EmployeeCountMax ??= employeeMax;
            account.LeadSource = LeadSource.Website;
            account.LeadPriority = priority;
            account.CurrentTimeTrackingSystem ??= Clean(request.CurrentSystem);
            account.IcpScore = request.OverallScore;
            account.PainHypothesis = BuildPainHypothesis(request);
            account.Notes = AppendNote(account.Notes, BuildAssessmentNote(request, now));
            account.UpdatedAt = now;
            await db.SaveChangesAsync();
        }

        await businessLineService.EnsureTaggedAsync(account.Id, BusinessLine.TimeOn);

        var contact = await db.Contacts
            .FirstOrDefaultAsync(c => c.AccountId == account.Id && c.Email == email);

        if (contact is null)
        {
            var (firstName, lastName) = SplitName(request.Name);
            contact = new Contact
            {
                AccountId = account.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = Clean(request.Phone),
                IsPrimary = !await db.Contacts.AnyAsync(c => c.AccountId == account.Id),
                PreferredChannel = !string.IsNullOrWhiteSpace(request.Phone)
                    ? PreferredContactChannel.Phone
                    : PreferredContactChannel.Email,
            };

            db.Contacts.Add(contact);
            await db.SaveChangesAsync();
        }
        else
        {
            contact.Phone ??= Clean(request.Phone);
            await db.SaveChangesAsync();
        }

        int? opportunityId = null;

        if (request.WantsContact)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Key == ProductKey.TimeOn);
            if (product is not null)
            {
                var existingOpportunity = await db.Opportunities
                    .Include(o => o.PipelineStage)
                    .FirstOrDefaultAsync(o =>
                        o.AccountId == account.Id
                        && o.ProductId == product.Id
                        && o.PipelineStage != null
                        && !o.PipelineStage.IsWon
                        && !o.PipelineStage.IsLost);

                if (existingOpportunity is null)
                {
                    var initialStageId = await db.PipelineStages
                        .Where(p => p.BusinessLine == BusinessLine.TimeOn)
                        .OrderBy(p => p.SortOrder)
                        .Select(p => p.Id)
                        .FirstOrDefaultAsync();

                    if (initialStageId != 0)
                    {
                        existingOpportunity = new Opportunity
                        {
                            AccountId = account.Id,
                            ContactId = contact.Id,
                            ProductId = product.Id,
                            PipelineStageId = initialStageId,
                            CreatedAt = now,
                        };

                        db.Opportunities.Add(existingOpportunity);
                        await db.SaveChangesAsync();
                    }
                }

                if (existingOpportunity is not null)
                {
                    opportunityId = existingOpportunity.Id;

                    var alreadyHasTask = await db.Activities.AnyAsync(a =>
                        a.OpportunityId == existingOpportunity.Id
                        && a.Status == ActivityStatus.Pending
                        && a.Subject.StartsWith("Revisar TimeOn Score"));

                    if (!alreadyHasTask)
                    {
                        db.Activities.Add(new Activity
                        {
                            AccountId = account.Id,
                            ContactId = contact.Id,
                            OpportunityId = existingOpportunity.Id,
                            BusinessLine = BusinessLine.TimeOn,
                            Type = ActivityType.Task,
                            Subject = $"Revisar TimeOn Score · {companyName}",
                            Description = $"Score {request.OverallScore}/100. Área más débil: {request.WeakestDimension}. El contacto ha solicitado revisión.",
                            DueDate = DateTime.UtcNow,
                            Status = ActivityStatus.Pending,
                        });
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        return Ok(new TimeOnScoreSubmitResponse
        {
            Accepted = true,
            AccountId = account.Id,
            ContactId = contact.Id,
            OpportunityId = opportunityId,
        });
    }

    private static bool IsValidScore(int score) => score is >= 0 and <= 100;

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (string FirstName, string LastName) SplitName(string name)
    {
        var parts = name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0
            ? ("", "")
            : parts.Length == 1
                ? (parts[0], "")
                : (parts[0], parts[1]);
    }

    private static (int? Min, int? Max) ParseTeamSize(string teamSize) => teamSize switch
    {
        "1-5" => (1, 5),
        "6-10" => (6, 10),
        "11-25" => (11, 25),
        "26-50" => (26, 50),
        "51-100" => (51, 100),
        "100+" => (101, null),
        _ => (null, null),
    };

    private static LeadPriority CalculatePriority(int overallScore, int? employeeMin, bool wantsContact)
    {
        if (wantsContact || (overallScore < 60 && employeeMin >= 11))
        {
            return LeadPriority.A;
        }

        return overallScore < 80 ? LeadPriority.B : LeadPriority.C;
    }

    private static string BuildPainHypothesis(TimeOnScoreSubmitRequest request)
        => $"TimeOn Score {request.OverallScore}/100. Principal oportunidad: {request.WeakestDimension}.";

    private static string BuildAssessmentNote(TimeOnScoreSubmitRequest request, DateTime timestamp)
        => $"[{timestamp:yyyy-MM-dd HH:mm} UTC] TimeOn Score {request.OverallScore}/100 | " +
           $"Registro {request.RegistrationScore} | Incidencias {request.IncidentsScore} | " +
           $"Administración {request.AdministrationScore} | Trazabilidad {request.TraceabilityScore} | " +
           $"Experiencia {request.ExperienceScore} | Equipo {request.TeamSize} | " +
           $"Sistema actual: {request.CurrentSystem ?? "No indicado"} | " +
           $"Solicita contacto: {(request.WantsContact ? "Sí" : "No")}";

    private static string AppendNote(string? existing, string note)
        => string.IsNullOrWhiteSpace(existing) ? note : $"{existing}\n{note}";
}

public sealed class TimeOnScoreSubmitRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [Required, MaxLength(200)]
    public string Company { get; set; } = "";

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = "";

    [MaxLength(40)]
    public string? Phone { get; set; }

    [Required, MaxLength(20)]
    public string TeamSize { get; set; } = "";

    public int OverallScore { get; set; }
    public int RegistrationScore { get; set; }
    public int IncidentsScore { get; set; }
    public int AdministrationScore { get; set; }
    public int TraceabilityScore { get; set; }
    public int ExperienceScore { get; set; }

    [Required, MaxLength(80)]
    public string WeakestDimension { get; set; } = "";

    [MaxLength(120)]
    public string? CurrentSystem { get; set; }

    public bool WantsContact { get; set; }

    // Honeypot. Debe quedar vacío en clientes reales.
    [MaxLength(200)]
    public string? Website { get; set; }
}

public sealed class TimeOnScoreSubmitResponse
{
    public bool Accepted { get; set; }
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? OpportunityId { get; set; }
}
