using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Controllers;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Tests;

public class SubscriptionsControllerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(Account account, Product product)> SeedAccountAndProductAsync(ApplicationDbContext db)
    {
        var account = new Account { AccountType = AccountType.Company, CompanyName = "DecisionLab Iberia" };
        var product = new Product { Key = ProductKey.DecisionLabRecruitment, Name = "DecisionLab - Recruitment", BusinessLine = BusinessLine.DecisionLab };
        db.Accounts.Add(account);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return (account, product);
    }

    [Fact]
    public async Task Create_PopulatesAccountAndProductNames()
    {
        await using var db = CreateContext();
        var (account, product) = await SeedAccountAndProductAsync(db);
        var controller = new SubscriptionsController(db, new AccountBusinessLineService(db));

        var created = await controller.Create(new SubscriptionDto
        {
            AccountId = account.Id,
            ProductId = product.Id,
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.Today,
            Quantity = 5,
        });

        var dto = Assert.IsType<SubscriptionDto>(((OkObjectResult)created.Result!).Value);
        Assert.Equal("DecisionLab Iberia", dto.AccountDisplayName);
        Assert.Equal("DecisionLab - Recruitment", dto.ProductName);
        Assert.Equal(BusinessLine.DecisionLab, dto.BusinessLine);
    }

    [Fact]
    public async Task GetAll_FiltersByBusinessLineAndStatus()
    {
        await using var db = CreateContext();
        var (account, product) = await SeedAccountAndProductAsync(db);
        var controller = new SubscriptionsController(db, new AccountBusinessLineService(db));

        await controller.Create(new SubscriptionDto { AccountId = account.Id, ProductId = product.Id, Status = SubscriptionStatus.Active, StartDate = DateTime.Today });
        await controller.Create(new SubscriptionDto { AccountId = account.Id, ProductId = product.Id, Status = SubscriptionStatus.Cancelled, StartDate = DateTime.Today });

        var active = await controller.GetAll(BusinessLine.DecisionLab, SubscriptionStatus.Active, null);
        var activeList = Assert.IsAssignableFrom<List<SubscriptionDto>>(active.Value);

        Assert.Single(activeList);
        Assert.Equal(SubscriptionStatus.Active, activeList[0].Status);
    }
}
