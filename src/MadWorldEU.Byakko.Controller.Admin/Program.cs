using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MadWorldEU.Byakko;
using MadWorldEU.Byakko.Startups;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.AddByakkoApiHttpClients();
builder.AddByakkoApplication();
builder.AddByakkoAuthentication();
builder.AddByakkoServices();

await builder.Build().RunAsync();