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
[Route("api/opportunities")]
public class OpportunitiesController(
    ApplicationDbContext db,
    ActivityRollupService rollupService,
    AccountBusinessLineService businessLineService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OpportunityDto>>> GetAll(
        [FromQuery] BusinessLine? businessLine,
        [FromQuery] int? pipelineStageId,
        [FromQuery] bool? missingNextAction,
        [FromQuery] int? accountId)
    {
        var query = db.Opportunities
            .Include(o => o.Account)
            .Include(o => o.Contact)
            .Include(o => o.Product)
            .Include(o => o.PipelineStage)
            .AsQueryable();

        if (businessLine.HasValue)
        {
            query = query.Where(o => o.Product!.BusinessLine == businessLine.Value);
        }

        if (pipelineStageId.HasValue)
        {
            query = query.Where(o => o.PipelineStageId == pipelineStageId.Value);
        }

        if (missingNextAction == true)
        {
            query = query.Where(o => o.PipelineStage != null && !o.PipelineStage.IsWon && !o.PipelineStage.IsLost
                && o.NextActionDueDate == null);
        }

        if (accountId.HasValue)
        {
            query = query.Where(o => o.AccountId == accountId.Value);
        }

        var opportunities = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return await ToDtosAsync(opportunities);
    }

    [HttpPost]
    public async Task<ActionResult<OpportunityDto>> Create(OpportunityCreateRequest request)
    {
        var product = await db.Products.FindAsync(request.ProductId);
        if (product is null)
        {
            return BadRequest("El producto indicado no existe.");
        }

        var pipelineStageId = request.PipelineStageId
            ?? (await db.PipelineStages
                .Where(p => p.BusinessLine == product.BusinessLine)
                .OrderBy(p => p.SortOrder)
                .Select(p => p.Id)
                .FirstOrDefaultAsync());

        if (pipelineStageId == 0)
        {
            return BadRequest("No hay etapas configuradas para la línea de negocio de este producto.");
        }

        var opportunity = new Opportunity
        {
            AccountId = request.AccountId,
            ContactId = request.ContactId,
            ProductId = request.ProductId,
            PipelineStageId = pipelineStageId,
            EstimatedValue = request.EstimatedValue,
            ExpectedCloseDate = request.ExpectedCloseDate,
            OwnerUserId = request.OwnerUserId,
            CreatedAt = DateTime.UtcNow,
        };

        db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync();

        opportunity.Product = product;
        await db.Entry(opportunity).Reference(o => o.Account).LoadAsync();
        await db.Entry(opportunity).Reference(o => o.PipelineStage).LoadAsync();
        if (opportunity.ContactId.HasValue)
        {
            await db.Entry(opportunity).Reference(o => o.Contact).LoadAsync();
        }

        await businessLineService.EnsureTaggedAsync(opportunity.AccountId, product.BusinessLine);

        if (!string.IsNullOrWhiteSpace(request.NextActionSubject) && request.NextActionDueDate.HasValue)
        {
            db.Activities.Add(new Activity
            {
                AccountId = opportunity.AccountId,
                ContactId = opportunity.ContactId,
                OpportunityId = opportunity.Id,
                BusinessLine = product.BusinessLine,
                Type = ActivityType.Task,
                Subject = request.NextActionSubject,
                DueDate = request.NextActionDueDate,
                Status = ActivityStatus.Pending,
                AssignedToUserId = request.OwnerUserId ?? userManager.GetUserId(User),
            });
            await db.SaveChangesAsync();

            await rollupService.RecomputeOpportunityNextActionAsync(opportunity.Id);
        }

        var dtos = await ToDtosAsync([opportunity]);
        return Ok(dtos[0]);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OpportunityUpdateRequest request)
    {
        var opportunity = await db.Opportunities.FindAsync(id);
        if (opportunity is null)
        {
            return NotFound();
        }

        opportunity.ContactId = request.ContactId;
        opportunity.PipelineStageId = request.PipelineStageId;
        opportunity.EstimatedValue = request.EstimatedValue;
        opportunity.ExpectedCloseDate = request.ExpectedCloseDate;
        opportunity.OwnerUserId = request.OwnerUserId;

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

    [HttpPost("maintenance/recompute-next-actions")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<object>> RecomputeNextActions()
    {
        var count = await rollupService.RecomputeAllOpenOpportunitiesNextActionAsync();
        return Ok(new { recomputed = count });
    }

    private async Task<List<OpportunityDto>> ToDtosAsync(List<Opportunity> opportunities)
    {
        var ownerIds = opportunities
            .Select(o => o.OwnerUserId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var ownerNames = ownerIds.Count == 0
            ? new Dictionary<string, string>()
            : await db.Users
                .Where(u => ownerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

        return opportunities.Select(o =>
        {
            var isOpen = o.PipelineStage is not null && !o.PipelineStage.IsWon && !o.PipelineStage.IsLost;

            return new OpportunityDto
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
                PipelineStageId = o.PipelineStageId,
                PipelineStageKey = o.PipelineStage?.Key ?? "",
                PipelineStageName = o.PipelineStage?.Name ?? "",
                IsWon = o.PipelineStage?.IsWon ?? false,
                IsLost = o.PipelineStage?.IsLost ?? false,
                IsOpen = isOpen,
                EstimatedValue = o.EstimatedValue,
                ExpectedCloseDate = o.ExpectedCloseDate,
                OwnerUserId = o.OwnerUserId,
                OwnerName = o.OwnerUserId is not null && ownerNames.TryGetValue(o.OwnerUserId, out var name) ? name : null,
                NextActionActivityId = o.NextActionActivityId,
                NextActionSubject = o.NextActionSubject,
                NextActionDueDate = o.NextActionDueDate,
                IsNextActionMissing = isOpen && o.NextActionDueDate == null,
                CreatedAt = o.CreatedAt,
            };
        }).ToList();
    }
}
