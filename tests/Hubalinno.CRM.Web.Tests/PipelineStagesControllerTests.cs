using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Controllers;
using Hubalinno.CRM.Web.Data;
using Hubalinno.CRM.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Tests;

public class PipelineStagesControllerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsStagesForRequestedLine_OrderedBySortOrder()
    {
        await using var db = CreateContext();
        db.PipelineStages.AddRange(
            new PipelineStage { BusinessLine = BusinessLine.TimeOn, Key = "Contacted", Name = "Contactado", SortOrder = 1 },
            new PipelineStage { BusinessLine = BusinessLine.TimeOn, Key = "Prospecting", Name = "Prospección", SortOrder = 0 },
            new PipelineStage { BusinessLine = BusinessLine.Academy, Key = "New", Name = "Nueva", SortOrder = 0 }
        );
        await db.SaveChangesAsync();

        var controller = new PipelineStagesController(db);
        var result = await controller.GetAll(BusinessLine.TimeOn);
        var stages = Assert.IsAssignableFrom<List<PipelineStageDto>>(result.Value);

        Assert.Equal(2, stages.Count);
        Assert.Equal("Prospección", stages[0].Name);
        Assert.Equal("Contactado", stages[1].Name);
    }
}
