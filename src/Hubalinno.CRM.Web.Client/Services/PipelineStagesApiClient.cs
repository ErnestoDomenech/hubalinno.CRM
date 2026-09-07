using System.Net.Http.Json;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class PipelineStagesApiClient(HttpClient http)
{
    public async Task<List<PipelineStageDto>> GetAllAsync(BusinessLine? businessLine = null)
    {
        var url = "api/pipeline-stages" + (businessLine.HasValue ? $"?businessLine={businessLine}" : "");
        return await http.GetFromJsonAsync<List<PipelineStageDto>>(url) ?? [];
    }
}
