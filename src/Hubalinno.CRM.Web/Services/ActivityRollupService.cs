using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Services;

/// <summary>
/// Mantiene sincronizados los campos "cache" derivados de las Activity: la próxima acción de una
/// Opportunity, el último contacto de un Contact y el último contacto de un AccountBusinessLine.
/// La fuente de verdad siempre es la tabla Activities; estos métodos solo la reflejan.
/// </summary>
public class ActivityRollupService(ApplicationDbContext db)
{
    public async Task RecomputeOpportunityNextActionAsync(int opportunityId)
    {
        var opportunity = await db.Opportunities.FindAsync(opportunityId);
        if (opportunity is null)
        {
            return;
        }

        var nextActivity = await db.Activities
            .Where(a => a.OpportunityId == opportunityId && a.Status == ActivityStatus.Pending && a.DueDate != null)
            .OrderBy(a => a.DueDate)
            .FirstOrDefaultAsync();

        opportunity.NextActionActivityId = nextActivity?.Id;
        opportunity.NextActionSubject = nextActivity?.Subject;
        opportunity.NextActionDueDate = nextActivity?.DueDate;

        await db.SaveChangesAsync();
    }

    public async Task RecomputeContactLastContactAsync(int contactId)
    {
        var contact = await db.Contacts.FindAsync(contactId);
        if (contact is null)
        {
            return;
        }

        contact.LastContactAt = await db.Activities
            .Where(a => a.ContactId == contactId && a.Status == ActivityStatus.Done && a.CompletedAt != null)
            .MaxAsync(a => (DateTime?)a.CompletedAt);

        await db.SaveChangesAsync();
    }

    public async Task RecomputeAccountBusinessLineLastContactedAsync(int accountId, BusinessLine businessLine)
    {
        var tag = await db.AccountBusinessLines
            .FirstOrDefaultAsync(l => l.AccountId == accountId && l.BusinessLine == businessLine);
        if (tag is null)
        {
            return;
        }

        tag.LastContactedAt = await db.Activities
            .Where(a => a.AccountId == accountId && a.BusinessLine == businessLine && a.Status == ActivityStatus.Done && a.CompletedAt != null)
            .MaxAsync(a => (DateTime?)a.CompletedAt);

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Herramienta de reparación: recalcula la próxima acción de todas las oportunidades abiertas.
    /// Útil tras una migración de datos o si se sospecha de una inconsistencia manual en BD.
    /// </summary>
    public async Task<int> RecomputeAllOpenOpportunitiesNextActionAsync()
    {
        var openOpportunityIds = await db.Opportunities
            .Where(o => o.PipelineStage != null && !o.PipelineStage.IsWon && !o.PipelineStage.IsLost)
            .Select(o => o.Id)
            .ToListAsync();

        foreach (var id in openOpportunityIds)
        {
            await RecomputeOpportunityNextActionAsync(id);
        }

        return openOpportunityIds.Count;
    }
}
