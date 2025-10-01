using TextParsingApi.Services;
using TextParsingApi.Services.Implementation;
using TextParsingApi.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Text Parsing API", Version = "v1" });
    // XML documentation is optional
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "TextParsingApi.xml");
    if (File.Exists(xmlFile))
    {
        c.IncludeXmlComments(xmlFile);
    }
});

// Register validation configuration
builder.Services.AddSingleton<ValidationConfiguration>();
builder.Services.AddScoped<ValidationRules>();

// Register our services
builder.Services.AddScoped<ITextParsingService, TextParsingService>();
builder.Services.AddScoped<IXmlParsingService, XmlParsingService>();
builder.Services.AddScoped<ITaxCalculationService, TaxCalculationService>();

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // In production, the React app is served from the same origin
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Add startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting TextParsingApp...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("URLs: {Urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "default");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Text Parsing API V1");
        c.RoutePrefix = "swagger";
    });
}

// Only use HTTPS redirection in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("ReactApp");

// Serve static files (React app)
app.UseStaticFiles();

// Add simple root health endpoint for Railway
app.MapGet("/health", () => new {
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    MachineName = Environment.MachineName,
    ProcessId = Environment.ProcessId
});

// Add diagnostic endpoint
app.MapGet("/ping", () => "pong");

app.UseAuthorization();
app.MapControllers();

// Fallback routing for React SPA
app.MapFallbackToFile("index.html");

logger.LogInformation("Application configured successfully. Starting web host...");

app.Run();
