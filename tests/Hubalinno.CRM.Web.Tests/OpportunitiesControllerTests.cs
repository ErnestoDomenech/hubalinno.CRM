using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Controllers;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Hubalinno.CRM.Web.Tests;

public class OpportunitiesControllerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationDbContext db) => new(
        new UserStore<ApplicationUser>(db),
        Options.Create(new IdentityOptions()),
        new PasswordHasher<ApplicationUser>(),
        [],
        [],
        new UpperInvariantLookupNormalizer(),
        new IdentityErrorDescriber(),
        null!,
        NullLogger<UserManager<ApplicationUser>>.Instance);

    private static OpportunitiesController CreateController(ApplicationDbContext db)
    {
        var controller = new OpportunitiesController(db, new ActivityRollupService(db), new AccountBusinessLineService(db), CreateUserManager(db));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) },
        };
        return controller;
    }

    private static async Task<(Account account, Product product, PipelineStage first, PipelineStage second)> SeedAsync(ApplicationDbContext db)
    {
        var account = new Account { AccountType = AccountType.Company, CompanyName = "TimeOn Prospect" };
        var product = new Product { Key = ProductKey.TimeOn, Name = "TimeOn", BusinessLine = BusinessLine.TimeOn };
        var first = new PipelineStage { BusinessLine = BusinessLine.TimeOn, Key = "Prospecting", Name = "Prospección", SortOrder = 0 };
        var second = new PipelineStage { BusinessLine = BusinessLine.TimeOn, Key = "Contacted", Name = "Contactado", SortOrder = 1 };
        db.AddRange(account, product, first, second);
        await db.SaveChangesAsync();
        return (account, product, first, second);
    }

    [Fact]
    public async Task Create_WithNextAction_CreatesLinkedActivityAndClearsIncompleteFlag()
    {
        await using var db = CreateContext();
        var (account, product, first, _) = await SeedAsync(db);
        var controller = CreateController(db);

        var created = await controller.Create(new OpportunityCreateRequest
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = first.Id,
            EstimatedValue = 5000,
            NextActionSubject = "Llamar mañana",
            NextActionDueDate = DateTime.Today.AddDays(1),
        });

        var dto = Assert.IsType<OpportunityDto>(((OkObjectResult)created.Result!).Value);
        Assert.False(dto.IsNextActionMissing);
        Assert.Equal("Llamar mañana", dto.NextActionSubject);

        var activityCount = await db.Activities.CountAsync(a => a.OpportunityId == dto.Id);
        Assert.Equal(1, activityCount);
    }

    [Fact]
    public async Task Create_WithoutNextAction_OpenOpportunityIsMarkedMissing()
    {
        await using var db = CreateContext();
        var (account, product, first, _) = await SeedAsync(db);
        var controller = CreateController(db);

        var created = await controller.Create(new OpportunityCreateRequest
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = first.Id,
            EstimatedValue = 3000,
        });

        var dto = Assert.IsType<OpportunityDto>(((OkObjectResult)created.Result!).Value);
        Assert.True(dto.IsNextActionMissing);
        Assert.Null(dto.NextActionDueDate);
    }

    [Fact]
    public async Task Create_TagsAccountIntoTheProductsBusinessLine()
    {
        await using var db = CreateContext();
        var (account, product, first, _) = await SeedAsync(db);
        var controller = CreateController(db);

        await controller.Create(new OpportunityCreateRequest
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = first.Id,
            EstimatedValue = 1000,
        });

        var tagged = await db.AccountBusinessLines.AnyAsync(l => l.AccountId == account.Id && l.BusinessLine == BusinessLine.TimeOn);
        Assert.True(tagged);
    }

    [Fact]
    public async Task Update_DoesNotAcceptNextActionFields_TheyStayDerivedOnly()
    {
        await using var db = CreateContext();
        var (account, product, first, second) = await SeedAsync(db);
        var controller = CreateController(db);

        var created = await controller.Create(new OpportunityCreateRequest
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = first.Id,
            EstimatedValue = 1000,
        });
        var id = ((OpportunityDto)((OkObjectResult)created.Result!).Value!).Id;

        // OpportunityUpdateRequest no expone NextAction*: solo puede cambiar la etapa/valor.
        await controller.Update(id, new OpportunityUpdateRequest
        {
            PipelineStageId = second.Id,
            EstimatedValue = 1500,
        });

        var updated = await db.Opportunities.FindAsync(id);
        Assert.Equal(second.Id, updated!.PipelineStageId);
        Assert.Equal(1500, updated.EstimatedValue);
        Assert.Null(updated.NextActionSubject);
    }

    [Fact]
    public async Task GetAll_FiltersByMissingNextAction()
    {
        await using var db = CreateContext();
        var (account, product, first, _) = await SeedAsync(db);
        var controller = CreateController(db);

        await controller.Create(new OpportunityCreateRequest { AccountId = account.Id, ProductId = product.Id, PipelineStageId = first.Id, EstimatedValue = 100 });
        await controller.Create(new OpportunityCreateRequest
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = first.Id,
            EstimatedValue = 200,
            NextActionSubject = "Seguimiento",
            NextActionDueDate = DateTime.Today,
        });

        var missing = await controller.GetAll(null, null, true, null);
        var list = Assert.IsAssignableFrom<List<OpportunityDto>>(missing.Value);

        Assert.Single(list);
        Assert.Equal(100, list[0].EstimatedValue);
    }

    [Fact]
    public async Task RecomputeNextActions_RepairsManuallyDesyncedOpportunity_WithoutDuplicatingActivities()
    {
        await using var db = CreateContext();
        var (account, product, first, _) = await SeedAsync(db);
        var controller = CreateController(db);

        var created = await controller.Create(new OpportunityCreateRequest
        {
            AccountId = account.Id,
            ProductId = product.Id,
            PipelineStageId = first.Id,
            EstimatedValue = 1000,
            NextActionSubject = "Original",
            NextActionDueDate = DateTime.Today.AddDays(2),
        });
        var id = ((OpportunityDto)((OkObjectResult)created.Result!).Value!).Id;

        // Desincronizar manualmente (simula una inconsistencia en BD).
        var opportunity = await db.Opportunities.FindAsync(id);
        opportunity!.NextActionSubject = "Desincronizado";
        opportunity.NextActionDueDate = DateTime.Today.AddDays(99);
        await db.SaveChangesAsync();

        var result = await controller.RecomputeNextActions();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);

        var repaired = await db.Opportunities.FindAsync(id);
        Assert.Equal("Original", repaired!.NextActionSubject);

        var activityCount = await db.Activities.CountAsync(a => a.OpportunityId == id);
        Assert.Equal(1, activityCount);
    }
}
