using System.Net.Http.Json;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class OpportunitiesApiClient(HttpClient http)
{
    public async Task<List<OpportunityDto>> GetAllAsync(BusinessLine? businessLine = null, OpportunityStage? stage = null, int? accountId = null)
    {
        var query = new List<string>();
        if (businessLine.HasValue) query.Add($"businessLine={businessLine}");
        if (stage.HasValue) query.Add($"stage={stage}");
        if (accountId.HasValue) query.Add($"accountId={accountId}");
        var url = "api/opportunities" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        return await http.GetFromJsonAsync<List<OpportunityDto>>(url) ?? [];
    }

    public async Task CreateAsync(OpportunityDto dto)
    {
        var response = await http.PostAsJsonAsync("api/opportunities", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(OpportunityDto dto)
    {
        var response = await http.PutAsJsonAsync($"api/opportunities/{dto.Id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/opportunities/{id}");
        response.EnsureSuccessStatusCode();
    }
}
