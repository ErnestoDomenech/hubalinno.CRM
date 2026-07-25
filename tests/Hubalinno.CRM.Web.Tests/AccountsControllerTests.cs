using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Controllers;
using Hubalinno.CRM.Web.Data;
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

    [Fact]
    public async Task Create_Then_GetById_ReturnsSameAccount()
    {
        await using var db = CreateContext();
        var controller = new AccountsController(db);

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
    public async Task GetAll_FiltersByAccountType()
    {
        await using var db = CreateContext();
        var controller = new AccountsController(db);

        await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Empresa X" });
        await controller.Create(new AccountDto { AccountType = AccountType.Individual, FirstName = "Ana", LastName = "Ruiz" });

        var companies = await controller.GetAll(AccountType.Company, null);
        var companiesResult = Assert.IsAssignableFrom<List<AccountDto>>(companies.Value);

        Assert.Single(companiesResult);
        Assert.Equal("Empresa X", companiesResult[0].CompanyName);
    }

    [Fact]
    public async Task Delete_RemovesAccount()
    {
        await using var db = CreateContext();
        var controller = new AccountsController(db);

        var created = await controller.Create(new AccountDto { AccountType = AccountType.Company, CompanyName = "Temporal" });
        var id = ((AccountDto)((CreatedAtActionResult)created.Result!).Value!).Id;

        await controller.Delete(id);

        var fetched = await controller.GetById(id);
        Assert.IsType<NotFoundResult>(fetched.Result);
    }
}
