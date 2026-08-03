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
[Route("api/subscriptions")]
public class SubscriptionsController(ApplicationDbContext db, AccountBusinessLineService businessLineService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SubscriptionDto>>> GetAll(
        [FromQuery] BusinessLine? businessLine,
        [FromQuery] SubscriptionStatus? status,
        [FromQuery] int? accountId)
    {
        var query = db.Subscriptions
            .Include(s => s.Account)
            .Include(s => s.Product)
            .AsQueryable();

        if (businessLine.HasValue)
        {
            query = query.Where(s => s.Product!.BusinessLine == businessLine.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(s => s.AccountId == accountId.Value);
        }

        var subscriptions = await query
            .OrderBy(s => s.RenewalDate)
            .ToListAsync();

        return subscriptions.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Create(SubscriptionDto dto)
    {
        var subscription = new Subscription();
        ApplyDto(subscription, dto);

        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync();

        await db.Entry(subscription).Reference(s => s.Account).LoadAsync();
        await db.Entry(subscription).Reference(s => s.Product).LoadAsync();

        await businessLineService.EnsureTaggedAsync(subscription.AccountId, subscription.Product!.BusinessLine);

        return Ok(ToDto(subscription));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SubscriptionDto dto)
    {
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription is null)
        {
            return NotFound();
        }

        ApplyDto(subscription, dto);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var subscription = await db.Subscriptions.FindAsync(id);
        if (subscription is null)
        {
            return NotFound();
        }

        db.Subscriptions.Remove(subscription);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static void ApplyDto(Subscription subscription, SubscriptionDto dto)
    {
        subscription.AccountId = dto.AccountId;
        subscription.ProductId = dto.ProductId;
        subscription.Status = dto.Status;
        subscription.StartDate = dto.StartDate;
        subscription.EndDate = dto.EndDate;
        subscription.RenewalDate = dto.RenewalDate;
        subscription.Quantity = dto.Quantity;
        subscription.Notes = dto.Notes;
    }

    private static SubscriptionDto ToDto(Subscription s) => new()
    {
        Id = s.Id,
        AccountId = s.AccountId,
        AccountDisplayName = s.Account!.AccountType == AccountType.Company
            ? s.Account.CompanyName ?? ""
            : $"{s.Account.FirstName} {s.Account.LastName}".Trim(),
        ProductId = s.ProductId,
        ProductName = s.Product!.Name,
        BusinessLine = s.Product.BusinessLine,
        Status = s.Status,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        RenewalDate = s.RenewalDate,
        Quantity = s.Quantity,
        Notes = s.Notes,
    };
}
