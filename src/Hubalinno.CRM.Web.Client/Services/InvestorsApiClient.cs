using System.Net.Http.Json;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class InvestorsApiClient(HttpClient http)
{
    public async Task<List<InvestorDto>> GetAllAsync()
        => await http.GetFromJsonAsync<List<InvestorDto>>("api/investors") ?? [];

    public async Task<InvestorDto?> GetByAccountIdAsync(int accountId)
        => await http.GetFromJsonAsync<InvestorDto>($"api/investors/{accountId}");

    public async Task<InvestorDto?> CreateAsync(InvestorDto dto)
    {
        var response = await http.PostAsJsonAsync("api/investors", dto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<InvestorDto>();
    }

    public async Task DeleteAsync(int accountId)
    {
        var response = await http.DeleteAsync($"api/investors/{accountId}");
        response.EnsureSuccessStatusCode();
    }
}