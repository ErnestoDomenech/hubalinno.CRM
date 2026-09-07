using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/contacts")]
public class ContactsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ContactDto>>> GetAll([FromQuery] int accountId)
    {
        var contacts = await db.Contacts
            .Where(c => c.AccountId == accountId)
            .OrderBy(c => c.LastName)
            .ToListAsync();

        return contacts.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<ContactDto>> Create(ContactDto dto)
    {
        var accountExists = await db.Accounts.AnyAsync(a => a.Id == dto.AccountId);
        if (!accountExists)
        {
            return BadRequest("La cuenta indicada no existe.");
        }

        var contact = new Contact();
        ApplyDto(contact, dto);

        db.Contacts.Add(contact);
        await db.SaveChangesAsync();

        return Ok(ToDto(contact));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ContactDto dto)
    {
        var contact = await db.Contacts.FindAsync(id);
        if (contact is null)
        {
            return NotFound();
        }

        ApplyDto(contact, dto);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await db.Contacts.FindAsync(id);
        if (contact is null)
        {
            return NotFound();
        }

        db.Contacts.Remove(contact);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static void ApplyDto(Contact contact, ContactDto dto)
    {
        contact.AccountId = dto.AccountId;
        contact.FirstName = dto.FirstName;
        contact.LastName = dto.LastName;
        contact.Email = dto.Email;
        contact.Phone = dto.Phone;
        contact.JobTitle = dto.JobTitle;
        contact.LinkedInUrl = dto.LinkedInUrl;
        contact.IsPrimary = dto.IsPrimary;
        contact.DecisionRole = dto.DecisionRole;
        contact.PreferredChannel = dto.PreferredChannel;
        // LastContactAt es un cache derivado (ActivityRollupService) y no se acepta del cliente.
    }

    private static ContactDto ToDto(Contact c) => new()
    {
        Id = c.Id,
        AccountId = c.AccountId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        Phone = c.Phone,
        JobTitle = c.JobTitle,
        LinkedInUrl = c.LinkedInUrl,
        IsPrimary = c.IsPrimary,
        DecisionRole = c.DecisionRole,
        PreferredChannel = c.PreferredChannel,
        LastContactAt = c.LastContactAt,
    };
}
