using System.Net.Http.Json;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class UsersApiClient(HttpClient http)
{
    public async Task<List<UserSummaryDto>> GetAllAsync()
        => await http.GetFromJsonAsync<List<UserSummaryDto>>("api/users") ?? [];
}
