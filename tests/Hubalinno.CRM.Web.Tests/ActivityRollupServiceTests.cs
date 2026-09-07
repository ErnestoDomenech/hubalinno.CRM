using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Tests;

public class ActivityRollupServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(Account account, Product product, PipelineStage openStage, Opportunity opportunity)> SeedOpenOpportunityAsync(ApplicationDbContext db)
    {
        var account = new Account { AccountType = AccountType.Company, CompanyName = "TimeOn Prospect" };
        var product = new Product { Key = ProductKey.TimeOn, Name = "TimeOn", BusinessLine = BusinessLine.TimeOn };
        var openStage = new PipelineStage { BusinessLine = BusinessLine.TimeOn, Key = "Contacted", Name = "Contactado", SortOrder = 1 };
        db.Accounts.Add(account);
        db.Products.Add(product);
        db.PipelineStages.Add(openStage);
        await db.SaveChangesAsync();

        var opportunity = new Opportunity
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = openStage.Id,
            EstimatedValue = 1000,
            CreatedAt = DateTime.UtcNow,
        };
        db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync();

        return (account, product, openStage, opportunity);
    }

    [Fact]
    public async Task RecomputeOpportunityNextAction_SetsFieldsFromEarliestPendingActivity()
    {
        await using var db = CreateContext();
        var (_, _, _, opportunity) = await SeedOpenOpportunityAsync(db);
        var service = new ActivityRollupService(db);

        db.Activities.Add(new Activity
        {
            OpportunityId = opportunity.Id,
            Type = ActivityType.Call,
            Subject = "Llamada de seguimiento",
            Status = ActivityStatus.Pending,
            DueDate = DateTime.Today.AddDays(3),
        });
        await db.SaveChangesAsync();

        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var updated = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal("Llamada de seguimiento", updated!.NextActionSubject);
        Assert.Equal(DateTime.Today.AddDays(3), updated.NextActionDueDate);
        Assert.NotNull(updated.NextActionActivityId);
    }

    [Fact]
    public async Task RecomputeOpportunityNextAction_PicksEarliestOfSeveralPendingActivities()
    {
        await using var db = CreateContext();
        var (_, _, _, opportunity) = await SeedOpenOpportunityAsync(db);
        var service = new ActivityRollupService(db);

        db.Activities.AddRange(
            new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Task, Subject = "Tarea lejana", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(10) },
            new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Call, Subject = "Llamada próxima", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(1) }
        );
        await db.SaveChangesAsync();

        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var updated = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal("Llamada próxima", updated!.NextActionSubject);
    }

    [Fact]
    public async Task CompletingTheNextActionActivity_RecomputesToNextPendingOrClears()
    {
        await using var db = CreateContext();
        var (_, _, _, opportunity) = await SeedOpenOpportunityAsync(db);
        var service = new ActivityRollupService(db);

        var first = new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Call, Subject = "Primera", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(1) };
        var second = new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Call, Subject = "Segunda", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(5) };
        db.Activities.AddRange(first, second);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        first.Status = ActivityStatus.Done;
        first.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var afterFirstDone = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal("Segunda", afterFirstDone!.NextActionSubject);

        second.Status = ActivityStatus.Done;
        second.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var afterBothDone = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Null(afterBothDone!.NextActionSubject);
        Assert.Null(afterBothDone.NextActionDueDate);
        Assert.Null(afterBothDone.NextActionActivityId);
    }

    [Fact]
    public async Task ReopeningACompletedActivity_CountsAgainAsNextActionIfEarliest()
    {
        await using var db = CreateContext();
        var (_, _, _, opportunity) = await SeedOpenOpportunityAsync(db);
        var service = new ActivityRollupService(db);

        var activity = new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Call, Subject = "Reabierta", Status = ActivityStatus.Done, CompletedAt = DateTime.UtcNow, DueDate = DateTime.Today.AddDays(2) };
        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var beforeReopen = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Null(beforeReopen!.NextActionSubject);

        activity.Status = ActivityStatus.Pending;
        activity.CompletedAt = null;
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var afterReopen = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal("Reabierta", afterReopen!.NextActionSubject);
    }

    [Fact]
    public async Task DeletingTheNextActionActivity_RecomputesToNextAvailable()
    {
        await using var db = CreateContext();
        var (_, _, _, opportunity) = await SeedOpenOpportunityAsync(db);
        var service = new ActivityRollupService(db);

        var toDelete = new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Call, Subject = "Se eliminará", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(1) };
        var remaining = new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Email, Subject = "Queda", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(4) };
        db.Activities.AddRange(toDelete, remaining);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        db.Activities.Remove(toDelete);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var updated = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal("Queda", updated!.NextActionSubject);
    }

    [Fact]
    public async Task ChangingTheDueDate_UpdatesNextAction()
    {
        await using var db = CreateContext();
        var (_, _, _, opportunity) = await SeedOpenOpportunityAsync(db);
        var service = new ActivityRollupService(db);

        var activity = new Activity { OpportunityId = opportunity.Id, Type = ActivityType.Call, Subject = "Con fecha", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(5) };
        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        activity.DueDate = DateTime.Today.AddDays(20);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunity.Id);

        var updated = await db.Opportunities.FindAsync(opportunity.Id);
        Assert.Equal(DateTime.Today.AddDays(20), updated!.NextActionDueDate);
    }

    [Fact]
    public async Task MovingActivityToAnotherOpportunity_RecomputesBothOriginAndDestination()
    {
        await using var db = CreateContext();
        var (account, product, stage, opportunityA) = await SeedOpenOpportunityAsync(db);
        var opportunityB = new Opportunity { AccountId = account.Id, ProductId = product.Id, PipelineStageId = stage.Id, EstimatedValue = 500, CreatedAt = DateTime.UtcNow };
        db.Opportunities.Add(opportunityB);
        await db.SaveChangesAsync();

        var service = new ActivityRollupService(db);
        var activity = new Activity { OpportunityId = opportunityA.Id, Type = ActivityType.Call, Subject = "Movible", Status = ActivityStatus.Pending, DueDate = DateTime.Today.AddDays(2) };
        db.Activities.Add(activity);
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunityA.Id);

        var beforeMoveA = await db.Opportunities.FindAsync(opportunityA.Id);
        Assert.Equal("Movible", beforeMoveA!.NextActionSubject);

        activity.OpportunityId = opportunityB.Id;
        await db.SaveChangesAsync();
        await service.RecomputeOpportunityNextActionAsync(opportunityA.Id);
        await service.RecomputeOpportunityNextActionAsync(opportunityB.Id);

        var afterMoveA = await db.Opportunities.FindAsync(opportunityA.Id);
        var afterMoveB = await db.Opportunities.FindAsync(opportunityB.Id);
        Assert.Null(afterMoveA!.NextActionSubject);
        Assert.Equal("Movible", afterMoveB!.NextActionSubject);
    }

    [Fact]
    public async Task RecomputeContactLastContact_UsesMostRecentCompletedActivity()
    {
        await using var db = CreateContext();
        var account = new Account { AccountType = AccountType.Company, CompanyName = "Cuenta" };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        var contact = new Contact { AccountId = account.Id, FirstName = "Laura", LastName = "Gómez" };
        db.Contacts.Add(contact);
        await db.SaveChangesAsync();

        var older = DateTime.UtcNow.AddDays(-5);
        var newer = DateTime.UtcNow.AddDays(-1);
        db.Activities.AddRange(
            new Activity { ContactId = contact.Id, Type = ActivityType.Call, Subject = "Antigua", Status = ActivityStatus.Done, CompletedAt = older },
            new Activity { ContactId = contact.Id, Type = ActivityType.Email, Subject = "Reciente", Status = ActivityStatus.Done, CompletedAt = newer }
        );
        await db.SaveChangesAsync();

        var service = new ActivityRollupService(db);
        await service.RecomputeContactLastContactAsync(contact.Id);

        var updated = await db.Contacts.FindAsync(contact.Id);
        Assert.Equal(newer, updated!.LastContactAt);
    }

    [Fact]
    public async Task RecomputeAccountBusinessLineLastContacted_OnlyCountsCompletedActivitiesForThatLine()
    {
        await using var db = CreateContext();
        var account = new Account { AccountType = AccountType.Company, CompanyName = "Cuenta TimeOn" };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var tag = new AccountBusinessLine { AccountId = account.Id, BusinessLine = BusinessLine.TimeOn, AddedAt = DateTime.UtcNow };
        db.AccountBusinessLines.Add(tag);
        await db.SaveChangesAsync();

        var contactedAt = DateTime.UtcNow.AddHours(-2);
        db.Activities.AddRange(
            new Activity { AccountId = account.Id, BusinessLine = BusinessLine.TimeOn, Type = ActivityType.Call, Subject = "TimeOn", Status = ActivityStatus.Done, CompletedAt = contactedAt },
            new Activity { AccountId = account.Id, BusinessLine = BusinessLine.Academy, Type = ActivityType.Call, Subject = "Otra línea", Status = ActivityStatus.Done, CompletedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = new ActivityRollupService(db);
        await service.RecomputeAccountBusinessLineLastContactedAsync(account.Id, BusinessLine.TimeOn);

        var updatedTag = await db.AccountBusinessLines.FindAsync(tag.Id);
        Assert.Equal(contactedAt, updatedTag!.LastContactedAt);
    }

    [Fact]
    public async Task RecomputeAllOpenOpportunitiesNextAction_SkipsWonAndLostOpportunities()
    {
        await using var db = CreateContext();
        var (account, product, openStage, openOpportunity) = await SeedOpenOpportunityAsync(db);
        var wonStage = new PipelineStage { BusinessLine = BusinessLine.TimeOn, Key = "Won", Name = "Ganada", SortOrder = 6, IsWon = true };
        db.PipelineStages.Add(wonStage);
        await db.SaveChangesAsync();

        var wonOpportunity = new Opportunity { AccountId = account.Id, ProductId = product.Id, PipelineStageId = wonStage.Id, EstimatedValue = 2000, CreatedAt = DateTime.UtcNow };
        db.Opportunities.Add(wonOpportunity);
        await db.SaveChangesAsync();

        var service = new ActivityRollupService(db);
        var recomputedCount = await service.RecomputeAllOpenOpportunitiesNextActionAsync();

        Assert.Equal(1, recomputedCount);
    }
}
