using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using MotChecker;
using MotChecker.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add memory cache
builder.Services.AddMemoryCache();

// Register Mock Service for development
builder.Services.AddScoped<IVehicleService, MockVehicleService>();

// Add logging
builder.Services.AddLogging(logging => logging
    .SetMinimumLevel(LogLevel.Information)
);

await builder.Build().RunAsync();