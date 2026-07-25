using Hubalinno.CRM.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var adminEmail = configuration["Seed:AdminEmail"] ?? "ideandogroup@gmail.com";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Hubalinno#2026";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Administrador Hubalinno",
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el usuario admin semilla: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }

        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Entities.Product { Key = ProductKey.AcademyProfessional, Name = "Academy - Profesional", BusinessLine = BusinessLine.Academy },
                new Entities.Product { Key = ProductKey.AcademyCompany, Name = "Academy - Empresa", BusinessLine = BusinessLine.Academy },
                new Entities.Product { Key = ProductKey.DecisionLabRecruitment, Name = "DecisionLab - Recruitment", BusinessLine = BusinessLine.DecisionLab },
                new Entities.Product { Key = ProductKey.DecisionLabPerformance, Name = "DecisionLab - Performance", BusinessLine = BusinessLine.DecisionLab },
                new Entities.Product { Key = ProductKey.TimeOn, Name = "TimeOn", BusinessLine = BusinessLine.TimeOn }
            );
            await dbContext.SaveChangesAsync();
        }
    }
}
