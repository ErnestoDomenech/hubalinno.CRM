using System.Net.Http.Json;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class ContactsApiClient(HttpClient http)
{
    public async Task<List<ContactDto>> GetByAccountAsync(int accountId)
        => await http.GetFromJsonAsync<List<ContactDto>>($"api/contacts?accountId={accountId}") ?? [];

    public async Task CreateAsync(ContactDto dto)
    {
        var response = await http.PostAsJsonAsync("api/contacts", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(ContactDto dto)
    {
        var response = await http.PutAsJsonAsync($"api/contacts/{dto.Id}", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/contacts/{id}");
        response.EnsureSuccessStatusCode();
    }
}
