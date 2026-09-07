using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountsController(ApplicationDbContext db, AccountBusinessLineService businessLineService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll(
        [FromQuery] AccountType? type,
        [FromQuery] string? search,
        [FromQuery] BusinessLine? businessLine,
        [FromQuery] LeadPriority? leadPriority,
        [FromQuery] ContactStatusFilter? contactStatus)
    {
        var query = db.Accounts.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(a => a.AccountType == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                (a.CompanyName != null && a.CompanyName.Contains(term)) ||
                (a.FirstName != null && a.FirstName.Contains(term)) ||
                (a.LastName != null && a.LastName.Contains(term)) ||
                (a.Email != null && a.Email.Contains(term)));
        }

        if (businessLine.HasValue)
        {
            query = query.Where(a => db.AccountBusinessLines.Any(l => l.AccountId == a.Id && l.BusinessLine == businessLine.Value));
        }

        if (leadPriority.HasValue)
        {
            query = query.Where(a => a.LeadPriority == leadPriority.Value);
        }

        if (contactStatus == ContactStatusFilter.ToContactToday)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            query = query.Where(a => db.Activities.Any(act =>
                act.AccountId == a.Id
                && act.Status == ActivityStatus.Pending
                && act.DueDate >= today && act.DueDate < tomorrow
                && (!businessLine.HasValue || act.BusinessLine == businessLine.Value)));
        }
        else if (contactStatus == ContactStatusFilter.NeverContacted)
        {
            query = query.Where(a => db.AccountBusinessLines.Any(l =>
                l.AccountId == a.Id
                && (!businessLine.HasValue || l.BusinessLine == businessLine.Value)
                && l.LastContactedAt == null));
        }

        var accounts = await query.OrderBy(a => a.CompanyName).ThenBy(a => a.LastName).ToListAsync();
        return accounts.Select(ToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDto>> GetById(int id)
    {
        var account = await db.Accounts.FindAsync(id);
        return account is null ? NotFound() : ToDto(account);
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(AccountDto dto)
    {
        var account = new Account();
        ApplyDto(account, dto);
        account.CreatedAt = DateTime.UtcNow;
        account.UpdatedAt = account.CreatedAt;

        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, ToDto(account));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AccountDto dto)
    {
        var account = await db.Accounts.FindAsync(id);
        if (account is null)
        {
            return NotFound();
        }

        ApplyDto(account, dto);
        account.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await db.Accounts.FindAsync(id);
        if (account is null)
        {
            return NotFound();
        }

        db.Accounts.Remove(account);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("No se puede eliminar la cuenta porque tiene oportunidades asociadas. Elimínalas primero.");
        }

        return NoContent();
    }

    [HttpPost("{id:int}/business-lines/{businessLine}")]
    public async Task<IActionResult> TagBusinessLine(int id, BusinessLine businessLine)
    {
        var account = await db.Accounts.FindAsync(id);
        if (account is null)
        {
            return NotFound();
        }

        await businessLineService.EnsureTaggedAsync(id, businessLine);
        return NoContent();
    }

    private static void ApplyDto(Account account, AccountDto dto)
    {
        account.AccountType = dto.AccountType;
        account.CompanyName = dto.CompanyName;
        account.FirstName = dto.FirstName;
        account.LastName = dto.LastName;
        account.TaxId = dto.TaxId;
        account.Email = dto.Email;
        account.Phone = dto.Phone;
        account.Address = dto.Address;
        account.City = dto.City;
        account.Country = dto.Country;
        account.Notes = dto.Notes;
        account.Website = dto.Website;
        account.LinkedInUrl = dto.LinkedInUrl;
        account.Industry = dto.Industry;
        account.EmployeeCountMin = dto.EmployeeCountMin;
        account.EmployeeCountMax = dto.EmployeeCountMax;
        account.Province = dto.Province;
        account.LeadPriority = dto.LeadPriority;
        account.LeadSource = dto.LeadSource;
        account.CurrentTimeTrackingSystem = dto.CurrentTimeTrackingSystem;
        account.PainHypothesis = dto.PainHypothesis;
        account.IcpScore = dto.IcpScore;
        account.DoNotContact = dto.DoNotContact;
    }

    private static AccountDto ToDto(Account a) => new()
    {
        Id = a.Id,
        AccountType = a.AccountType,
        CompanyName = a.CompanyName,
        FirstName = a.FirstName,
        LastName = a.LastName,
        TaxId = a.TaxId,
        Email = a.Email,
        Phone = a.Phone,
        Address = a.Address,
        City = a.City,
        Country = a.Country,
        Notes = a.Notes,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Website = a.Website,
        LinkedInUrl = a.LinkedInUrl,
        Industry = a.Industry,
        EmployeeCountMin = a.EmployeeCountMin,
        EmployeeCountMax = a.EmployeeCountMax,
        Province = a.Province,
        LeadPriority = a.LeadPriority,
        LeadSource = a.LeadSource,
        CurrentTimeTrackingSystem = a.CurrentTimeTrackingSystem,
        PainHypothesis = a.PainHypothesis,
        IcpScore = a.IcpScore,
        DoNotContact = a.DoNotContact,
    };
}
