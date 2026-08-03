using System.Net.Http.Json;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class OpportunitiesApiClient(HttpClient http)
{
    public async Task<List<OpportunityDto>> GetAllAsync(
        BusinessLine? businessLine = null,
        int? pipelineStageId = null,
        bool? missingNextAction = null,
        int? accountId = null)
    {
        var query = new List<string>();
        if (businessLine.HasValue) query.Add($"businessLine={businessLine}");
        if (pipelineStageId.HasValue) query.Add($"pipelineStageId={pipelineStageId}");
        if (missingNextAction.HasValue) query.Add($"missingNextAction={missingNextAction.Value.ToString().ToLowerInvariant()}");
        if (accountId.HasValue) query.Add($"accountId={accountId}");
        var url = "api/opportunities" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        return await http.GetFromJsonAsync<List<OpportunityDto>>(url) ?? [];
    }

    public async Task<OpportunityDto?> CreateAsync(OpportunityCreateRequest request)
    {
        var response = await http.PostAsJsonAsync("api/opportunities", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OpportunityDto>();
    }

    public async Task UpdateAsync(int id, OpportunityUpdateRequest request)
    {
        var response = await http.PutAsJsonAsync($"api/opportunities/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/opportunities/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<int> RecomputeNextActionsAsync()
    {
        var response = await http.PostAsync("api/opportunities/maintenance/recompute-next-actions", null);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RecomputeResult>();
        return result?.Recomputed ?? 0;
    }

    private class RecomputeResult
    {
        public int Recomputed { get; set; }
    }
}
