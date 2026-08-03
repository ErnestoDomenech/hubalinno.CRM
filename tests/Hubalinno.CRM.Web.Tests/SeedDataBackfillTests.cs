using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Tests;

public class SeedDataBackfillTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task BackfillOpportunityPipelineStages_LinksLegacyStageToMatchingPipelineStage()
    {
        await using var db = CreateContext();

        var account = new Account { AccountType = AccountType.Company, CompanyName = "Cuenta legacy" };
        var product = new Product { Key = ProductKey.DecisionLabRecruitment, Name = "DecisionLab - Recruitment", BusinessLine = BusinessLine.DecisionLab };
        db.AddRange(account, product);
        await db.SaveChangesAsync();

        // Oportunidad "legacy": solo tiene el enum antiguo, sin PipelineStageId (como quedarían las
        // filas existentes de Academy/DecisionLab justo después de aplicar la migración de esquema).
        var legacyOpportunity = new Opportunity
        {
            AccountId = account.Id,
            ProductId = product.Id,
            Stage = OpportunityStage.Negotiation,
            PipelineStageId = null,
            EstimatedValue = 1000,
            CreatedAt = DateTime.UtcNow,
        };
        db.Opportunities.Add(legacyOpportunity);
        await db.SaveChangesAsync();

        await SeedData.SeedPipelineStagesAsync(db);
        await SeedData.BackfillOpportunityPipelineStagesAsync(db);

        var updated = await db.Opportunities
            .Include(o => o.PipelineStage)
            .SingleAsync(o => o.Id == legacyOpportunity.Id);

        Assert.NotNull(updated.PipelineStageId);
        Assert.Equal("Negotiation", updated.PipelineStage!.Key);
        Assert.Equal(BusinessLine.DecisionLab, updated.PipelineStage.BusinessLine);
    }

    [Fact]
    public async Task BackfillOpportunityPipelineStages_IsIdempotent_DoesNotReassignAlreadyLinkedRows()
    {
        await using var db = CreateContext();

        var account = new Account { AccountType = AccountType.Company, CompanyName = "Cuenta" };
        var product = new Product { Key = ProductKey.TimeOn, Name = "TimeOn", BusinessLine = BusinessLine.TimeOn };
        db.AddRange(account, product);
        await db.SaveChangesAsync();

        await SeedData.SeedPipelineStagesAsync(db);

        var manualStage = await db.PipelineStages.FirstAsync(p => p.BusinessLine == BusinessLine.TimeOn);
        var opportunity = new Opportunity
        {
            AccountId = account.Id,
            ProductId = product.Id,
            Stage = OpportunityStage.New,
            PipelineStageId = manualStage.Id,
            EstimatedValue = 500,
            CreatedAt = DateTime.UtcNow,
        };
        db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync();

        await SeedData.BackfillOpportunityPipelineStagesAsync(db);

        var reloaded = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal(manualStage.Id, reloaded!.PipelineStageId);
    }
}
