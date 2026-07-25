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
[Route("api/opportunities")]
public class OpportunitiesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OpportunityDto>>> GetAll(
        [FromQuery] BusinessLine? businessLine,
        [FromQuery] OpportunityStage? stage,
        [FromQuery] int? accountId)
    {
        var query = db.Opportunities
            .Include(o => o.Account)
            .Include(o => o.Contact)
            .Include(o => o.Product)
            .AsQueryable();

        if (businessLine.HasValue)
        {
            query = query.Where(o => o.Product!.BusinessLine == businessLine.Value);
        }

        if (stage.HasValue)
        {
            query = query.Where(o => o.Stage == stage.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(o => o.AccountId == accountId.Value);
        }

        var opportunities = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return opportunities.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<OpportunityDto>> Create(OpportunityDto dto)
    {
        var opportunity = new Opportunity { CreatedAt = DateTime.UtcNow };
        ApplyDto(opportunity, dto);

        db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync();

        await db.Entry(opportunity).Reference(o => o.Account).LoadAsync();
        await db.Entry(opportunity).Reference(o => o.Product).LoadAsync();
        if (opportunity.ContactId.HasValue)
        {
            await db.Entry(opportunity).Reference(o => o.Contact).LoadAsync();
        }

        return Ok(ToDto(opportunity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OpportunityDto dto)
    {
        var opportunity = await db.Opportunities.FindAsync(id);
        if (opportunity is null)
        {
            return NotFound();
        }

        ApplyDto(opportunity, dto);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var opportunity = await db.Opportunities.FindAsync(id);
        if (opportunity is null)
        {
            return NotFound();
        }

        db.Opportunities.Remove(opportunity);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static void ApplyDto(Opportunity opportunity, OpportunityDto dto)
    {
        opportunity.AccountId = dto.AccountId;
        opportunity.ContactId = dto.ContactId;
        opportunity.ProductId = dto.ProductId;
        opportunity.Stage = dto.Stage;
        opportunity.EstimatedValue = dto.EstimatedValue;
        opportunity.ExpectedCloseDate = dto.ExpectedCloseDate;
    }

    private static OpportunityDto ToDto(Opportunity o) => new()
    {
        Id = o.Id,
        AccountId = o.AccountId,
        AccountDisplayName = o.Account!.AccountType == AccountType.Company
            ? o.Account.CompanyName ?? ""
            : $"{o.Account.FirstName} {o.Account.LastName}".Trim(),
        ContactId = o.ContactId,
        ContactName = o.Contact is null ? null : $"{o.Contact.FirstName} {o.Contact.LastName}".Trim(),
        ProductId = o.ProductId,
        ProductName = o.Product!.Name,
        BusinessLine = o.Product.BusinessLine,
        Stage = o.Stage,
        EstimatedValue = o.EstimatedValue,
        ExpectedCloseDate = o.ExpectedCloseDate,
        CreatedAt = o.CreatedAt,
    };
}
