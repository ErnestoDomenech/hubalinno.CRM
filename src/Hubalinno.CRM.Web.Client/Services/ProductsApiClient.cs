using System.Net.Http.Json;
using Hubalinno.CRM.Shared.Dtos;

namespace Hubalinno.CRM.Web.Client.Services;

public class ProductsApiClient(HttpClient http)
{
    public async Task<List<ProductDto>> GetAllAsync()
        => await http.GetFromJsonAsync<List<ProductDto>>("api/products") ?? [];
}
