using System.Net.Http.Json;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class ActivitiesApiClient(HttpClient http)
{
    public async Task<List<ActivityDto>> GetAllAsync(int? accountId = null, int? opportunityId = null, ActivityStatus? status = null, bool onlyMine = false)
    {
        var query = new List<string>();
        if (accountId.HasValue) query.Add($"accountId={accountId}");
        if (opportunityId.HasValue) query.Add($"opportunityId={opportunityId}");
        if (status.HasValue) query.Add($"status={status}");
        if (onlyMine) query.Add("onlyMine=true");
        var url = "api/activities" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        return await http.GetFromJsonAsync<List<ActivityDto>>(url) ?? [];
    }

    public async Task CreateAsync(ActivityDto dto)
    {
        var response = await http.PostAsJsonAsync("api/activities", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(ActivityDto dto)
    {
        var response = await http.PutAsJsonAsync($"api/activities/{dto.Id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/activities/{id}");
        response.EnsureSuccessStatusCode();
    }
}
