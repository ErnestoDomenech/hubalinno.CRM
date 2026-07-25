using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public class ActivitiesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ActivityDto>>> GetAll(
        [FromQuery] int? accountId,
        [FromQuery] int? opportunityId,
        [FromQuery] ActivityStatus? status,
        [FromQuery] bool onlyMine = false)
    {
        var query = db.Activities
            .Include(a => a.Account)
            .Include(a => a.AssignedToUser)
            .AsQueryable();

        if (accountId.HasValue)
        {
            query = query.Where(a => a.AccountId == accountId.Value);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(a => a.OpportunityId == opportunityId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (onlyMine)
        {
            var currentUserId = userManager.GetUserId(User);
            query = query.Where(a => a.AssignedToUserId == currentUserId);
        }

        var activities = await query
            .OrderBy(a => a.DueDate)
            .ToListAsync();

        return activities.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(ActivityDto dto)
    {
        var activity = new Activity();
        ApplyDto(activity, dto);

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

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

        ApplyDto(activity, dto);
        await db.SaveChangesAsync();

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

        db.Activities.Remove(activity);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActivityDto> LoadDtoAsync(int id)
    {
        var activity = await db.Activities
            .Include(a => a.Account)
            .Include(a => a.AssignedToUser)
            .FirstAsync(a => a.Id == id);

        return ToDto(activity);
    }

    private static void ApplyDto(Activity activity, ActivityDto dto)
    {
        activity.AccountId = dto.AccountId;
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
        OpportunityId = a.OpportunityId,
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
