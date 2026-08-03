using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Controllers;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Hubalinno.CRM.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Tests;

public class AccountsControllerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static AccountsController CreateController(ApplicationDbContext db) =>
        new(db, new AccountBusinessLineService(db));

    [Fact]
    public async Task Create_Then_GetById_ReturnsSameAccount()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        var created = await controller.Create(new AccountDto
        {
            AccountType = AccountType.Company,
            CompanyName = "Acme S.L.",
            Email = "info@acme.com",
        });

        var createdDto = Assert.IsType<AccountDto>(((CreatedAtActionResult)created.Result!).Value);
        Assert.True(createdDto.Id > 0);

        var fetched = await controller.GetById(createdDto.Id);
        var fetchedDto = Assert.IsType<AccountDto>(((ActionResult<AccountDto>)fetched).Value);
        Assert.Equal("Acme S.L.", fetchedDto.CompanyName);
    }

    [Fact]
    public async Task Create_PersistsProspectingFields()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        var created = await controller.Create(new AccountDto
        {
            AccountType = AccountType.Company,
            CompanyName = "Prospecto TimeOn",
            Website = "https://prospecto.example",
            LeadPriority = LeadPriority.A,
            LeadSource = LeadSource.Outbound,
            IcpScore = 80,
            DoNotContact = false,
        });

        var dto = Assert.IsType<AccountDto>(((CreatedAtActionResult)created.Result!).Value);
        Assert.Equal(LeadPriority.A, dto.LeadPriority);
        Assert.Equal(LeadSource.Outbound, dto.LeadSource);
        Assert.Equal(80, dto.IcpScore);
    }

    [Fact]
    public async Task GetAll_FiltersByAccountType()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Empresa X" });
        await controller.Create(new AccountDto { AccountType = AccountType.Individual, FirstName = "Ana", LastName = "Ruiz" });

        var companies = await controller.GetAll(AccountType.Company, null, null, null, null);
        var companiesResult = Assert.IsAssignableFrom<List<AccountDto>>(companies.Value);

        Assert.Single(companiesResult);
        Assert.Equal("Empresa X", companiesResult[0].CompanyName);
    }

    [Fact]
    public async Task GetAll_FiltersByBusinessLine_ViaAccountBusinessLineTag()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        var timeOnLead = await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Lead TimeOn" });
        var timeOnLeadId = ((AccountDto)((CreatedAtActionResult)timeOnLead.Result!).Value!).Id;
        await controller.TagBusinessLine(timeOnLeadId, BusinessLine.TimeOn);

        await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Sin línea" });

        var timeOnAccounts = await controller.GetAll(null, null, BusinessLine.TimeOn, null, null);
        var result = Assert.IsAssignableFrom<List<AccountDto>>(timeOnAccounts.Value);

        Assert.Single(result);
        Assert.Equal("Lead TimeOn", result[0].CompanyName);
    }

    [Fact]
    public async Task GetAll_FiltersByLeadPriority()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Prioridad A", LeadPriority = LeadPriority.A });
        await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Prioridad C", LeadPriority = LeadPriority.C });

        var priorityA = await controller.GetAll(null, null, null, LeadPriority.A, null);
        var result = Assert.IsAssignableFrom<List<AccountDto>>(priorityA.Value);

        Assert.Single(result);
        Assert.Equal("Prioridad A", result[0].CompanyName);
    }

    [Fact]
    public async Task GetAll_ContactStatusNeverContacted_ReturnsTaggedAccountsWithoutLastContact()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        var created = await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Nunca contactada" });
        var id = ((AccountDto)((CreatedAtActionResult)created.Result!).Value!).Id;
        await controller.TagBusinessLine(id, BusinessLine.TimeOn);

        var result = await controller.GetAll(null, null, BusinessLine.TimeOn, null, ContactStatusFilter.NeverContacted);
        var list = Assert.IsAssignableFrom<List<AccountDto>>(result.Value);

        Assert.Single(list);

        // Simular que ya se le ha contactado: deja de aparecer en "sin contactar".
        var tag = await db.AccountBusinessLines.SingleAsync(l => l.AccountId == id);
        tag.LastContactedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var afterContact = await controller.GetAll(null, null, BusinessLine.TimeOn, null, ContactStatusFilter.NeverContacted);
        Assert.Empty(Assert.IsAssignableFrom<List<AccountDto>>(afterContact.Value));
    }

    [Fact]
    public async Task TagBusinessLine_IsIdempotent()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        var created = await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Empresa" });
        var id = ((AccountDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        await controller.TagBusinessLine(id, BusinessLine.TimeOn);
        await controller.TagBusinessLine(id, BusinessLine.TimeOn);

        var tags = await db.AccountBusinessLines.Where(l => l.AccountId == id).ToListAsync();
        Assert.Single(tags);
    }

    [Fact]
    public async Task Delete_RemovesAccount()
    {
        await using var db = CreateContext();
        var controller = CreateController(db);

        var created = await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Temporal" });
        var id = ((AccountDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        await controller.Delete(id);

        var fetched = await controller.GetById(id);
        Assert.IsType<NotFoundResult>(fetched.Result);
    }
}
