using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var soonThreshold = DateTime.Today.AddDays(30);
        var currentUserId = userManager.GetUserId(User);

        var summary = new DashboardSummaryDto
        {
            TotalAccounts = await db.Accounts.CountAsync(),
            PendingActivitiesMine = await db.Activities.CountAsync(a =>
                a.Status == ActivityStatus.Pending && a.AssignedToUserId == currentUserId),
        };

        foreach (var line in Enum.GetValues<BusinessLine>())
        {
            var activeSubs = await db.Subscriptions
                .Where(s => s.Product!.BusinessLine == line && s.Status == SubscriptionStatus.Active)
                .CountAsync();

            var expiringSoon = await db.Subscriptions
                .Where(s => s.Product!.BusinessLine == line
                    && s.Status == SubscriptionStatus.Active
                    && s.RenewalDate != null
                    && s.RenewalDate <= soonThreshold)
                .CountAsync();

            var openOpps = db.Opportunities
                .Where(o => o.Product!.BusinessLine == line
                    && o.Stage != OpportunityStage.Won
                    && o.Stage != OpportunityStage.Lost);

            summary.Lines.Add(new BusinessLineSummaryDto
            {
                BusinessLine = line,
                ActiveSubscriptions = activeSubs,
                ExpiringSoon = expiringSoon,
                OpenOpportunities = await openOpps.CountAsync(),
                OpenOpportunitiesValue = await openOpps.SumAsync(o => o.EstimatedValue),
            });
        }

        return summary;
    }
}
