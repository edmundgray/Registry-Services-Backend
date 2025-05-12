// --- Start of Program.cs ---
using Microsoft.EntityFrameworkCore;
using RegistryApi.Data; // Adjust if your namespace differs
using RegistryApi.Repositories; // Adjust if your namespace differs
using RegistryApi.Services; // Adjust if your namespace differs

var builder = WebApplication.CreateBuilder(args);

// --- Service Configuration ---

// 1. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("RegistryDatabaseConnection");
// Add robust null-checking for connection string
ArgumentNullException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
builder.Services.AddDbContext<RegistryDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure AutoMapper
// Assumes SpecificationProfile is in the same assembly as Program.cs
builder.Services.AddAutoMapper(typeof(Program));

// 3. Register Repositories and Services
builder.Services.AddScoped<ISpecificationIdentifyingInformationRepository, SpecificationIdentifyingInformationRepository>();
builder.Services.AddScoped<ISpecificationCoreRepository, SpecificationCoreRepository>();
builder.Services.AddScoped<ISpecificationExtensionComponentRepository, SpecificationExtensionComponentRepository>();
builder.Services.AddScoped<ISpecificationService, SpecificationService>();

// 4. Add Controllers and API Explorer/Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => // Optional: Add Swagger info
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Registry API",
        Description = "API for managing data specifications"
    });
});


// --- Build the application ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // Optional: Configure Swagger UI
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Registry API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at app root
    });
    // Use developer exception page for detailed errors in development
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Add Authentication/Authorization middleware if needed here
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers(); // Maps attribute-routed controllers

app.Run();
// --- End of Program.cs ---

