using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Web.Data.Entities;
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
                new Product { Key = ProductKey.AcademyProfessional, Name = "Academy - Profesional", BusinessLine = BusinessLine.Academy },
                new Product { Key = ProductKey.AcademyCompany, Name = "Academy - Empresa", BusinessLine = BusinessLine.Academy },
                new Product { Key = ProductKey.DecisionLabRecruitment, Name = "DecisionLab - Recruitment", BusinessLine = BusinessLine.DecisionLab },
                new Product { Key = ProductKey.DecisionLabPerformance, Name = "DecisionLab - Performance", BusinessLine = BusinessLine.DecisionLab },
                new Product { Key = ProductKey.TimeOn, Name = "TimeOn", BusinessLine = BusinessLine.TimeOn }
            );
            await dbContext.SaveChangesAsync();
        }

        await SeedPipelineStagesAsync(dbContext);
        await BackfillOpportunityPipelineStagesAsync(dbContext);
        await BackfillAccountBusinessLinesAsync(dbContext);
    }

    /// <summary>
    /// Definición fija de etapas por línea de negocio. Academy y DecisionLab reproducen exactamente
    /// las 6 etapas del enum global heredado (mismas keys/orden/etiquetas); TimeOn tiene su propio
    /// flujo de 8 etapas ("Sales OS"). Añadir una línea nueva o una etapa nueva no requiere migración
    /// de enum, solo una fila más aquí.
    /// </summary>
    private static readonly Dictionary<BusinessLine, (string Key, string Name, int SortOrder, bool IsWon, bool IsLost)[]> StageDefinitions = new()
    {
        [BusinessLine.Academy] = SharedStages(),
        [BusinessLine.DecisionLab] = SharedStages(),
        [BusinessLine.TimeOn] =
        [
            ("Prospecting", "Prospección", 0, false, false),
            ("Contacted", "Contactado", 1, false, false),
            ("Conversation", "Conversación", 2, false, false),
            ("Demo", "Demo", 3, false, false),
            ("ProposalSent", "Propuesta enviada", 4, false, false),
            ("Negotiation", "Negociación", 5, false, false),
            ("Won", "Ganada", 6, true, false),
            ("Lost", "Perdida", 7, false, true),
        ],
    };

    private static (string, string, int, bool, bool)[] SharedStages() =>
    [
        ("New", "Nueva", 0, false, false),
        ("Qualified", "Calificada", 1, false, false),
        ("ProposalSent", "Propuesta enviada", 2, false, false),
        ("Negotiation", "Negociación", 3, false, false),
        ("Won", "Ganada", 4, true, false),
        ("Lost", "Perdida", 5, false, true),
    ];

    internal static async Task SeedPipelineStagesAsync(ApplicationDbContext dbContext)
    {
        foreach (var (businessLine, stages) in StageDefinitions)
        {
            var alreadySeeded = await dbContext.PipelineStages.AnyAsync(p => p.BusinessLine == businessLine);
            if (alreadySeeded)
            {
                continue;
            }

            foreach (var (key, name, sortOrder, isWon, isLost) in stages)
            {
                dbContext.PipelineStages.Add(new PipelineStage
                {
                    BusinessLine = businessLine,
                    Key = key,
                    Name = name,
                    SortOrder = sortOrder,
                    IsWon = isWon,
                    IsLost = isLost,
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }

    internal static async Task BackfillOpportunityPipelineStagesAsync(ApplicationDbContext dbContext)
    {
        var pendingOpportunities = await dbContext.Opportunities
            .Include(o => o.Product)
            .Where(o => o.PipelineStageId == null)
            .ToListAsync();

        if (pendingOpportunities.Count == 0)
        {
            return;
        }

        var stagesByLine = await dbContext.PipelineStages.ToListAsync();

        foreach (var opportunity in pendingOpportunities)
        {
            var businessLine = opportunity.Product!.BusinessLine;
            var legacyKey = opportunity.Stage.ToString();

            var matchingStage = stagesByLine.FirstOrDefault(s => s.BusinessLine == businessLine && s.Key == legacyKey);
            if (matchingStage is not null)
            {
                opportunity.PipelineStageId = matchingStage.Id;
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task BackfillAccountBusinessLinesAsync(ApplicationDbContext dbContext)
    {
        var existingTags = await dbContext.AccountBusinessLines
            .Select(l => new { l.AccountId, l.BusinessLine })
            .ToListAsync();
        var existingSet = existingTags.Select(t => (t.AccountId, t.BusinessLine)).ToHashSet();

        var opportunityPairs = await dbContext.Opportunities
            .Include(o => o.Product)
            .Select(o => new { o.AccountId, BusinessLine = o.Product!.BusinessLine })
            .Distinct()
            .ToListAsync();

        var subscriptionPairs = await dbContext.Subscriptions
            .Include(s => s.Product)
            .Select(s => new { s.AccountId, BusinessLine = s.Product!.BusinessLine })
            .Distinct()
            .ToListAsync();

        var missingPairs = opportunityPairs
            .Concat(subscriptionPairs)
            .Select(p => (p.AccountId, p.BusinessLine))
            .Distinct()
            .Where(p => !existingSet.Contains(p))
            .ToList();

        if (missingPairs.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var (accountId, businessLine) in missingPairs)
        {
            dbContext.AccountBusinessLines.Add(new AccountBusinessLine
            {
                AccountId = accountId,
                BusinessLine = businessLine,
                AddedAt = now,
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
