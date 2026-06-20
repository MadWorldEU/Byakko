using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MadWorldEU.Byakko;
using MadWorldEU.Byakko.Localization;
using MadWorldEU.Byakko.Startups;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddLocalization();

builder.AddByakkoApiHttpClients();
builder.AddByakkoAuthentication();
builder.AddByakkoServices();

var host = builder.Build();

var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
var savedCulture = await jsInterop.InvokeAsync<string>("getCulture");

if (!string.IsNullOrEmpty(savedCulture))
{
    if (!SupportedLanguages.Labels.Keys.Contains(savedCulture))
    {
        savedCulture = SupportedLanguages.DefaultLanguage;
    };
    
    var cultureInfo = new CultureInfo(savedCulture);
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
}

await host.RunAsync();