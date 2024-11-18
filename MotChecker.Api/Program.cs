using MotChecker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DVSA API proxy
builder.Services.AddHttpClient<DvsaApiProxy>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DvsaApi:BaseUrl"]!);
});
builder.Services.AddScoped<DvsaApiProxy>();

// Configure CORS - Allow Blazor app access
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:7029") // Blazor project URL
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Development middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Request pipeline
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();