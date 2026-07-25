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
[Route("api/accounts")]
public class AccountsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll([FromQuery] AccountType? type, [FromQuery] string? search)
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
    };
}
