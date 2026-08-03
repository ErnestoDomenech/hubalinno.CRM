using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;
using Hubalinno.CRM.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hubalinno.CRM.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/pipeline-stages")]
public class PipelineStagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PipelineStageDto>>> GetAll([FromQuery] BusinessLine? businessLine)
    {
        var query = db.PipelineStages.AsQueryable();

        if (businessLine.HasValue)
        {
            query = query.Where(p => p.BusinessLine == businessLine.Value);
        }

        var stages = await query.OrderBy(p => p.BusinessLine).ThenBy(p => p.SortOrder).ToListAsync();

        return stages.Select(p => new PipelineStageDto
        {
            Id = p.Id,
            BusinessLine = p.BusinessLine,
            Key = p.Key,
            Name = p.Name,
            SortOrder = p.SortOrder,
            IsWon = p.IsWon,
            IsLost = p.IsLost,
        }).ToList();
    }
}
