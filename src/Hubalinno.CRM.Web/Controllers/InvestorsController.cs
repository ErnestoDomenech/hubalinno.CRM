using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/investors")]
public class InvestorsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InvestorDto>>> GetAll()
    {
        var investors = await db.InvestorProfiles
            .AsNoTracking()
            .Include(i => i.Account)
                .ThenInclude(a => a!.Contacts)
            .OrderBy(i => i.Account!.CompanyName)
            .ToListAsync();

        return investors.Select(ToDto).ToList();
    }

    [HttpGet("{accountId:int}")]
    public async Task<ActionResult<InvestorDto>> GetByAccountId(int accountId)
    {
        var investor = await db.InvestorProfiles
            .AsNoTracking()
            .Include(i => i.Account)
                .ThenInclude(a => a!.Contacts)
            .FirstOrDefaultAsync(i => i.AccountId == accountId);

        return investor is null
            ? NotFound()
            : ToDto(investor);
    }

    [HttpPost]
    public async Task<ActionResult<InvestorDto>> Create(InvestorDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.InvestorName))
        {
            return BadRequest("El nombre del inversor es obligatorio.");
        }

        var account = new Account
        {
            AccountType = AccountType.Company,
            AccountCategory = AccountCategory.Investor,
            CompanyName = dto.InvestorName.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var investorProfile = new InvestorProfile
        {
            Account = account,

            InvestmentStage = dto.InvestmentStage,

            TypicalTicketMin = dto.TypicalTicketMin,
            TypicalTicketMax = dto.TypicalTicketMax,

            Thesis = dto.Thesis,
            Geography = dto.Geography,

            ThesisFit = dto.ThesisFit,
            AccessScore = dto.AccessScore,

            PortfolioConflict = dto.PortfolioConflict,
            WarmIntroRoute = dto.WarmIntroRoute,

            RelationshipTemperature = dto.RelationshipTemperature,
            PipelineStage = dto.PipelineStage,

            LastInteraction = dto.LastInteraction,

            NextAction = dto.NextAction,
            NextActionDate = dto.NextActionDate
        };

        db.InvestorProfiles.Add(investorProfile);

        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetByAccountId),
            new { accountId = account.Id },
            ToDto(investorProfile));
    }

    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> Delete(int accountId)
    {
        var investor = await db.InvestorProfiles
            .Include(i => i.Account)
            .FirstOrDefaultAsync(i => i.AccountId == accountId);

        if (investor is null)
        {
            return NotFound();
        }

        if (investor.Account is not null)
        {
            db.Accounts.Remove(investor.Account);
        }
        else
        {
            db.InvestorProfiles.Remove(investor);
        }

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{accountId:int}")]
    public async Task<IActionResult> Update(int accountId, InvestorDto dto)
    {
        var investor = await db.InvestorProfiles
            .Include(i => i.Account)
            .FirstOrDefaultAsync(i => i.AccountId == accountId);

        if (investor is null)
        {
            return NotFound();
        }

        if (investor.Account is not null)
        {
            investor.Account.CompanyName = dto.InvestorName?.Trim();
            investor.Account.UpdatedAt = DateTime.UtcNow;
            investor.Account.AccountCategory = AccountCategory.Investor;
        }

        investor.InvestmentStage = dto.InvestmentStage;
        investor.TypicalTicketMin = dto.TypicalTicketMin;
        investor.TypicalTicketMax = dto.TypicalTicketMax;
        investor.Thesis = dto.Thesis;
        investor.Geography = dto.Geography;
        investor.ThesisFit = dto.ThesisFit;
        investor.AccessScore = dto.AccessScore;
        investor.PortfolioConflict = dto.PortfolioConflict;
        investor.WarmIntroRoute = dto.WarmIntroRoute;
        investor.RelationshipTemperature = dto.RelationshipTemperature;
        investor.PipelineStage = dto.PipelineStage;
        investor.LastInteraction = dto.LastInteraction;
        investor.NextAction = dto.NextAction;
        investor.NextActionDate = dto.NextActionDate;

        await db.SaveChangesAsync();

        return NoContent();
    }

    private static InvestorDto ToDto(InvestorProfile investor)
    {
        var primaryContact = investor.Account?.Contacts
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .FirstOrDefault();

        return new InvestorDto
        {
            AccountId = investor.AccountId,
            InvestorProfileId = investor.Id,

            InvestorName = investor.Account?.CompanyName ?? "",

            PrimaryContactId = primaryContact?.Id,
            PrimaryContactName = primaryContact is null
                ? null
                : $"{primaryContact.FirstName} {primaryContact.LastName}".Trim(),

            InvestmentStage = investor.InvestmentStage,

            TypicalTicketMin = investor.TypicalTicketMin,
            TypicalTicketMax = investor.TypicalTicketMax,

            Thesis = investor.Thesis,
            Geography = investor.Geography,

            ThesisFit = investor.ThesisFit,
            AccessScore = investor.AccessScore,

            PortfolioConflict = investor.PortfolioConflict,
            WarmIntroRoute = investor.WarmIntroRoute,

            RelationshipTemperature = investor.RelationshipTemperature,

            PipelineStage = investor.PipelineStage,

            LastInteraction = investor.LastInteraction,

            NextAction = investor.NextAction,
            NextActionDate = investor.NextActionDate
        };
    }
}