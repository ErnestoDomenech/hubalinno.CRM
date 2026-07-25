using System.Net.Http.Json;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class SubscriptionsApiClient(HttpClient http)
{
    public async Task<List<SubscriptionDto>> GetAllAsync(BusinessLine? businessLine = null, SubscriptionStatus? status = null, int? accountId = null)
    {
        var query = new List<string>();
        if (businessLine.HasValue) query.Add($"businessLine={businessLine}");
        if (status.HasValue) query.Add($"status={status}");
        if (accountId.HasValue) query.Add($"accountId={accountId}");
        var url = "api/subscriptions" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        return await http.GetFromJsonAsync<List<SubscriptionDto>>(url) ?? [];
    }

    public async Task CreateAsync(SubscriptionDto dto)
    {
        var response = await http.PostAsJsonAsync("api/subscriptions", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(SubscriptionDto dto)
    {
        var response = await http.PutAsJsonAsync($"api/subscriptions/{dto.Id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/subscriptions/{id}");
        response.EnsureSuccessStatusCode();
    }
}
