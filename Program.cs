// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

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

// Container hosts (Railway, Docker) supply the listening port via PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

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
builder.Services.AddScoped<IAdditionalRequirementRepository, AdditionalRequirementRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserGroupRepository, UserGroupRepository>();
builder.Services.AddScoped<ICoreInvoiceModelRepository, CoreInvoiceModelRepository>(); // Add CoreInvoiceModelRepository
builder.Services.AddScoped<IExtensionComponentsModelHeaderRepository, ExtensionComponentsModelHeaderRepository>(); // Added
builder.Services.AddScoped<IExtensionComponentModelElementRepository, ExtensionComponentModelElementRepository>(); // Added

// 4. Register Services
builder.Services.AddScoped<ISpecificationService, SpecificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>(); // Register the placeholder password hasher

// --- START: CORS Configuration ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder => // Renamed 'builder' to 'policyBuilder' to avoid conflict with WebApplicationBuilder
        {
            // Allow any port on 127.0.0.1 or localhost and define methods/headers
            //policyBuilder.SetIsOriginAllowed(origin => 
            //            origin.StartsWith("http://127.0.0.1:") || 
            //            origin.StartsWith("http://localhost:"))
            //       .WithMethods("POST", "GET", "OPTIONS", "PUT", "DELETE") // Explicitly list allowed methods
            //       .WithHeaders("Content-Type", "Accept", "Authorization"); // Explicitly list allowed headers
            policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
});
// --- END: CORS Configuration ---

// --- START: JWT Authentication Configuration ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var isLocalEnvironment = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing");

// No signing key is committed to the repo. Locally, fall back to a random per-process key
// (tokens simply stop working after a restart); everywhere else a real key must be supplied
// via the JwtSettings__SecretKey environment variable or a secret store.
if (string.IsNullOrEmpty(secretKey) && isLocalEnvironment)
{
    secretKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    builder.Configuration["JwtSettings:SecretKey"] = secretKey;
}

// Keys that have been published in this repository's history must never be accepted outside local use.
string[] publishedKeys =
{
    "RegistryBackendSecretkeywhichmustbeatleast32byteslong",
    "YOUR_PRODUCTION_JWT_SECRET_KEY_HERE_MUST_BE_AT_LEAST_32_BYTES_LONG",
    "LocalDockerJwtSecretKeyAtLeast32BytesLong!!"
};

if (string.IsNullOrEmpty(secretKey)
    || Encoding.UTF8.GetByteCount(secretKey) < 32
    || (!isLocalEnvironment && publishedKeys.Contains(secretKey)))
{
    // Refuse to start rather than sign tokens with a missing, weak or publicly known key.
    throw new InvalidOperationException(
        "JwtSettings:SecretKey must be set to a private random value of at least 32 bytes (e.g. the JwtSettings__SecretKey environment variable).");
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
        Description = "API for managing eInvoice Specifications conformant to EN 16931"
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

// Apply EF Core migrations on startup when requested (used by Docker/Railway deployments)
if (!app.Environment.IsEnvironment("Testing") && app.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<RegistryDbContext>().Database.Migrate();
}

// Behind a TLS-terminating proxy, trust X-Forwarded-* so scheme/client IP are correct
var behindProxy = app.Configuration.GetValue<bool>("BehindProxy");
if (behindProxy)
{
    var forwardedOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                         | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    };
    forwardedOptions.KnownNetworks.Clear();
    forwardedOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedOptions);
}

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Add a more generic error handler for production
    app.UseExceptionHandler("/error"); // Example, create an /error endpoint
    app.UseHsts();
}

// The proxy terminates TLS, so the container itself only serves HTTP
if (!behindProxy)
{
    app.UseHttpsRedirection();
}

// --- START: Add CORS Middleware ---
// IMPORTANT: UseCors must be placed correctly, typically after UseRouting (implicitly added by MapControllers)
// and before UseAuthentication and UseAuthorization.
// For simplicity with minimal APIs or standard controller setup, placing it here is common.
app.UseCors("AllowSpecificOrigin");
// --- END: Add CORS Middleware ---


// --- START: Add Authentication and Authorization Middleware ---
// IMPORTANT: UseAuthentication must come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();
// --- END: Add Authentication and Authorization Middleware ---

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapControllers();

app.Run();
// --- End of Program.cs ---
