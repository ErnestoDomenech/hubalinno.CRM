using System.Globalization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Hubalinno.CRM.Web.Client.Services;

var culture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<AccountsApiClient>();
builder.Services.AddScoped<ContactsApiClient>();
builder.Services.AddScoped<ProductsApiClient>();
builder.Services.AddScoped<SubscriptionsApiClient>();
builder.Services.AddScoped<OpportunitiesApiClient>();
builder.Services.AddScoped<ActivitiesApiClient>();
builder.Services.AddScoped<UsersApiClient>();
builder.Services.AddScoped<DashboardApiClient>();
builder.Services.AddScoped<PipelineStagesApiClient>();

await builder.Build().RunAsync();
