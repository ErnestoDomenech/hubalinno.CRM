using System.Net.Http.Json;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class DashboardApiClient(HttpClient http)
{
    public async Task<DashboardSummaryDto?> GetSummaryAsync()
        => await http.GetFromJsonAsync<DashboardSummaryDto>("api/dashboard/summary");
}
