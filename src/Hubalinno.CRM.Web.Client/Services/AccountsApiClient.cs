using System.Net.Http.Json;
using Hubalinno.CRM.Shared;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class AccountsApiClient(HttpClient http)
{
    public async Task<List<AccountDto>> GetAllAsync(
    AccountType? type = null,
    string? search = null,
    BusinessLine? businessLine = null,
    LeadPriority? leadPriority = null,
    ContactStatusFilter? contactStatus = null,
    AccountCategory? category = null)
    {
        var query = new List<string>();

        if (type.HasValue)
            query.Add($"type={type}");

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");

        if (businessLine.HasValue)
            query.Add($"businessLine={businessLine}");

        if (leadPriority.HasValue)
            query.Add($"leadPriority={leadPriority}");

        if (contactStatus.HasValue)
            query.Add($"contactStatus={contactStatus}");

        if (category.HasValue)
            query.Add($"category={category}");

        var url = "api/accounts" +
                  (query.Count > 0 ? "?" + string.Join("&", query) : "");

        return await http.GetFromJsonAsync<List<AccountDto>>(url) ?? [];
    }

    public async Task<AccountDto?> GetByIdAsync(int id)
        => await http.GetFromJsonAsync<AccountDto>($"api/accounts/{id}");

    public async Task<AccountDto?> CreateAsync(AccountDto dto)
    {
        var response = await http.PostAsJsonAsync("api/accounts", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AccountDto>();
    }

    public async Task UpdateAsync(AccountDto dto)
    {
        var response = await http.PutAsJsonAsync($"api/accounts/{dto.Id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/accounts/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task TagBusinessLineAsync(int accountId, BusinessLine businessLine)
    {
        var response = await http.PostAsync($"api/accounts/{accountId}/business-lines/{businessLine}", null);
        response.EnsureSuccessStatusCode();
    }
}
