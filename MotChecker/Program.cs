using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using MotChecker;
using MotChecker.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to use MotChecker API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7276/") // API project URL
});

// Add memory cache
builder.Services.AddMemoryCache();

// Mock service available for development/testing
// To use mock data instead of real API, uncomment:
// builder.Services.AddScoped<IVehicleService, MockVehicleService>();

// Real service implementation
builder.Services.AddScoped<IVehicleService, VehicleService>();

// Add logging
builder.Services.AddLogging(logging => logging
    .SetMinimumLevel(LogLevel.Information)
);

await builder.Build().RunAsync();