using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Services;

/// <summary>
/// Mantiene la relación explícita Account &lt;-&gt; BusinessLine (tabla AccountBusinessLine),
/// independiente de que ya exista una Subscription u Opportunity para esa cuenta en esa línea.
/// </summary>
public class AccountBusinessLineService(ApplicationDbContext db)
{
    public async Task<AccountBusinessLine> EnsureTaggedAsync(int accountId, BusinessLine businessLine)
    {
        var existing = await db.AccountBusinessLines
            .FirstOrDefaultAsync(l => l.AccountId == accountId && l.BusinessLine == businessLine);

        if (existing is not null)
        {
            return existing;
        }

        var tag = new AccountBusinessLine
        {
            AccountId = accountId,
            BusinessLine = businessLine,
            AddedAt = DateTime.UtcNow,
        };

        db.AccountBusinessLines.Add(tag);
        await db.SaveChangesAsync();

        return tag;
    }
}
