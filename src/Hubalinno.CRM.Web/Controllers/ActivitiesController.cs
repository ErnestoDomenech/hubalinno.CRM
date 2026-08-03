using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public class ActivitiesController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    ActivityRollupService rollupService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ActivityDto>>> GetAll(
        [FromQuery] int? accountId,
        [FromQuery] int? contactId,
        [FromQuery] int? opportunityId,
        [FromQuery] ActivityStatus? status,
        [FromQuery] BusinessLine? businessLine,
        [FromQuery] ActivityBucket? bucket,
        [FromQuery] bool onlyMine = false)
    {
        var query = db.Activities
            .Include(a => a.Account)
            .Include(a => a.Contact)
            .Include(a => a.AssignedToUser)
            .AsQueryable();

        if (accountId.HasValue)
        {
            query = query.Where(a => a.AccountId == accountId.Value);
        }

        if (contactId.HasValue)
        {
            query = query.Where(a => a.ContactId == contactId.Value);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(a => a.OpportunityId == opportunityId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (businessLine.HasValue)
        {
            query = query.Where(a => a.BusinessLine == businessLine.Value);
        }

        if (bucket.HasValue)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            query = bucket.Value switch
            {
                ActivityBucket.Today => query.Where(a => a.Status == ActivityStatus.Pending && a.DueDate != null && a.DueDate >= today && a.DueDate < tomorrow),
                ActivityBucket.Overdue => query.Where(a => a.Status == ActivityStatus.Pending && a.DueDate != null && a.DueDate < today),
                ActivityBucket.Next7Days => query.Where(a => a.Status == ActivityStatus.Pending && a.DueDate != null && a.DueDate >= tomorrow && a.DueDate < today.AddDays(8)),
                ActivityBucket.Completed => query.Where(a => a.Status == ActivityStatus.Done),
                _ => query,
            };
        }

        if (onlyMine)
        {
            var currentUserId = userManager.GetUserId(User);
            query = query.Where(a => a.AssignedToUserId == currentUserId);
        }

        var orderedQuery = bucket == ActivityBucket.Completed
            ? query.OrderByDescending(a => a.CompletedAt)
            : query.OrderBy(a => a.DueDate);

        var activities = await orderedQuery.ToListAsync();
        return activities.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(ActivityDto dto)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var activity = new Activity();
        ApplyDto(activity, dto);
        await ResolveBusinessLineAsync(activity, dto);

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        await RecomputeAllAsync(activity.OpportunityId, activity.ContactId, activity.AccountId, activity.BusinessLine);

        await transaction.CommitAsync();

        return Ok(await LoadDtoAsync(activity.Id));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ActivityDto dto)
    {
        var activity = await db.Activities.FindAsync(id);
        if (activity is null)
        {
            return NotFound();
        }

        var oldOpportunityId = activity.OpportunityId;
        var oldContactId = activity.ContactId;
        var oldAccountId = activity.AccountId;
        var oldBusinessLine = activity.BusinessLine;

        await using var transaction = await db.Database.BeginTransactionAsync();

        ApplyDto(activity, dto);
        await ResolveBusinessLineAsync(activity, dto);

        await db.SaveChangesAsync();

        await RecomputeAllAsync(oldOpportunityId, oldContactId, oldAccountId, oldBusinessLine);
        await RecomputeAllAsync(activity.OpportunityId, activity.ContactId, activity.AccountId, activity.BusinessLine);

        await transaction.CommitAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var activity = await db.Activities.FindAsync(id);
        if (activity is null)
        {
            return NotFound();
        }

        var opportunityId = activity.OpportunityId;
        var contactId = activity.ContactId;
        var accountId = activity.AccountId;
        var businessLine = activity.BusinessLine;

        await using var transaction = await db.Database.BeginTransactionAsync();

        db.Activities.Remove(activity);
        await db.SaveChangesAsync();

        await RecomputeAllAsync(opportunityId, contactId, accountId, businessLine);

        await transaction.CommitAsync();

        return NoContent();
    }

    private async Task RecomputeAllAsync(int? opportunityId, int? contactId, int? accountId, BusinessLine? businessLine)
    {
        if (opportunityId.HasValue)
        {
            await rollupService.RecomputeOpportunityNextActionAsync(opportunityId.Value);
        }

        if (contactId.HasValue)
        {
            await rollupService.RecomputeContactLastContactAsync(contactId.Value);
        }

        if (accountId.HasValue && businessLine.HasValue)
        {
            await rollupService.RecomputeAccountBusinessLineLastContactedAsync(accountId.Value, businessLine.Value);
        }
    }

    /// <summary>
    /// Si el cliente no informa una línea de negocio explícita, se hereda de la oportunidad
    /// vinculada (si la hay). Permite registrar actividades "sueltas" desde TimeOn Sales OS
    /// (sin oportunidad todavía) indicando la línea a mano.
    /// </summary>
    private async Task ResolveBusinessLineAsync(Activity activity, ActivityDto dto)
    {
        if (dto.BusinessLine.HasValue)
        {
            activity.BusinessLine = dto.BusinessLine;
            return;
        }

        if (activity.OpportunityId.HasValue)
        {
            activity.BusinessLine = await db.Opportunities
                .Where(o => o.Id == activity.OpportunityId.Value)
                .Select(o => (BusinessLine?)o.Product!.BusinessLine)
                .FirstOrDefaultAsync();
        }
        else
        {
            activity.BusinessLine = null;
        }
    }

    private async Task<ActivityDto> LoadDtoAsync(int id)
    {
        var activity = await db.Activities
            .Include(a => a.Account)
            .Include(a => a.Contact)
            .Include(a => a.AssignedToUser)
            .FirstAsync(a => a.Id == id);

        return ToDto(activity);
    }

    private static void ApplyDto(Activity activity, ActivityDto dto)
    {
        activity.AccountId = dto.AccountId;
        activity.ContactId = dto.ContactId;
        activity.OpportunityId = dto.OpportunityId;
        activity.Type = dto.Type;
        activity.Subject = dto.Subject;
        activity.Description = dto.Description;
        activity.DueDate = dto.DueDate;
        activity.AssignedToUserId = dto.AssignedToUserId;

        if (activity.Status != ActivityStatus.Done && dto.Status == ActivityStatus.Done)
        {
            activity.CompletedAt = DateTime.UtcNow;
        }
        else if (dto.Status == ActivityStatus.Pending)
        {
            activity.CompletedAt = null;
        }

        activity.Status = dto.Status;
    }

    private static ActivityDto ToDto(Activity a) => new()
    {
        Id = a.Id,
        AccountId = a.AccountId,
        AccountDisplayName = a.Account is null
            ? null
            : (a.Account.AccountType == AccountType.Company ? a.Account.CompanyName : $"{a.Account.FirstName} {a.Account.LastName}".Trim()),
        ContactId = a.ContactId,
        ContactName = a.Contact is null ? null : $"{a.Contact.FirstName} {a.Contact.LastName}".Trim(),
        OpportunityId = a.OpportunityId,
        BusinessLine = a.BusinessLine,
        Type = a.Type,
        Subject = a.Subject,
        Description = a.Description,
        DueDate = a.DueDate,
        CompletedAt = a.CompletedAt,
        Status = a.Status,
        AssignedToUserId = a.AssignedToUserId,
        AssignedToName = a.AssignedToUser?.FullName,
    };
}
