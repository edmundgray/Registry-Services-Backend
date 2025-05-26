// --- Start of Program.cs ---
using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.Repositories;
using RegistryApi.Services; // Ensure this namespace is correct for your services
using Microsoft.Extensions.Logging;// Required for ILogger
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault for Staging and Production environments
if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
{
    var keyVaultUriString = Environment.GetEnvironmentVariable("VaultUri");
    if (!string.IsNullOrEmpty(keyVaultUriString) && Uri.TryCreate(keyVaultUriString, UriKind.Absolute, out var keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
    }
    else
    {
        // Optionally, log a warning or throw an exception if the VaultUri is missing in Staging/Production
        // For example: builder.Logging.AddConsole().CreateLogger<Program>().LogWarning("VaultUri environment variable is not set or invalid for {Environment} environment.", builder.Environment.EnvironmentName);
    }
}

// --- Service Configuration ---

// 1. Configure DbContext (Conditionally)
// Check if the environment is "Testing" (or any specific environment you use for tests)
if (builder.Environment.IsEnvironment("Testing"))
{
    // For integration tests, DbContext will be configured by CustomWebApplicationFactory.
    // No need to register SQL Server DbContext here.
    // The CustomWebApplicationFactory will add the InMemory provider.
    // You could log here if desired:
    // builder.Logging.AddConsole().AddDebug(); // Example
    // var logger = LoggerFactory.Create(logBuilder => logBuilder.AddConsole()).CreateLogger<Program>();
    // logger.LogInformation("Running in Testing environment. Skipping SQL Server DbContext registration.");
}
else
{
    // Original DbContext configuration for Development, Production, etc.
    var connectionString = builder.Configuration.GetConnectionString("RegistryDatabaseConnection");
    ArgumentNullException.ThrowIfNullOrEmpty(connectionString, nameof(connectionString)); // This line was causing the issue in tests
    builder.Services.AddDbContext<RegistryDbContext>(options =>
        options.UseSqlServer(connectionString));
}

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

// --- START: JWT Authentication Configuration ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    // It's critical to have a secret key. Throw an exception if it's not found.
    // This ensures the application doesn't start in an insecure state.
    throw new InvalidOperationException("JWT SecretKey is not configured. Please set it in appsettings.json or user secrets.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(); // Add Authorization services
// --- END: JWT Authentication Configuration ---

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
    // --- START: Add JWT Authentication to Swagger ---
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field (e.g., Bearer {token})",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
    {
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});
    // --- END: Add JWT Authentication to Swagger ---
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

// --- START: Add Authentication and Authorization Middleware ---
// IMPORTANT: UseAuthentication must come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();
// --- END: Add Authentication and Authorization Middleware ---

app.MapControllers();

app.Run();
// --- End of Program.cs ---
