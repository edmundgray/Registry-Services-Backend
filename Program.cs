// --- Start of Program.cs ---
using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.Repositories;
using RegistryApi.Services; // Ensure this namespace is correct for your services
using Microsoft.Extensions.Logging; // Required for ILogger

var builder = WebApplication.CreateBuilder(args);

// --- Service Configuration ---

// 1. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("RegistryDatabaseConnection");
ArgumentNullException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
builder.Services.AddDbContext<RegistryDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program)); // Assumes SpecificationProfile is in the same assembly

// 3. Register Repositories
builder.Services.AddScoped<ISpecificationIdentifyingInformationRepository, SpecificationIdentifyingInformationRepository>();
builder.Services.AddScoped<ISpecificationCoreRepository, SpecificationCoreRepository>();
builder.Services.AddScoped<ISpecificationExtensionComponentRepository, SpecificationExtensionComponentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();

// 4. Register Services
builder.Services.AddScoped<ISpecificationService, SpecificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>(); // Register the placeholder password hasher

// 5. Add Controllers and API Explorer/Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Registry API",
        Description = "API for managing data specifications"
    });
    // Later, for JWT: Add security definitions for Swagger
});

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();


// --- Build the application ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Registry API v1");
        
    });
    app.UseDeveloperExceptionPage();
}
else
{
    // Add a more generic error handler for production
    app.UseExceptionHandler("/error"); // Example, create an /error endpoint
    app.UseHsts();
}

app.UseHttpsRedirection();

// Authentication & Authorization middleware will go here in Phase 7
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
// --- End of Program.cs ---
