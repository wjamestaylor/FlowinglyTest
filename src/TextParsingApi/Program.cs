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
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TextParsingApi.xml"));
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
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

app.UseAuthorization();
app.MapControllers();

// Fallback routing for React SPA
app.MapFallbackToFile("index.html");

app.Run();
