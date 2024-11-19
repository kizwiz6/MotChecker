using Microsoft.Extensions.Options;
using MotChecker.Api.Middleware;
using MotChecker.Api.Options;
using MotChecker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Development-only configuration
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Bind and validate DVSA API options
builder.Services.AddOptions<DvsaApiOptions>()
    .Bind(builder.Configuration.GetSection(DvsaApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register DvsaApiProxy with options
builder.Services.AddHttpClient<DvsaApiProxy>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<DvsaApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

// Add other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add error handling middleware early in the pipeline
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BlazorApp");
app.UseAuthorization();
app.MapControllers();

app.Run();