/// <summary>
/// API Gateway implementation using Ocelot framework.
/// This file configures and sets up a complete API Gateway that routes requests to microservices.
/// </summary>

// Required namespaces for building an API Gateway
using Microsoft.AspNetCore.Builder;          // Provides access to application building and middleware configuration
using Microsoft.AspNetCore.Hosting;          // Provides hosting mechanisms for ASP.NET Core applications
using Microsoft.Extensions.Configuration;    // Provides configuration management from JSON files, environment variables, etc.
using Microsoft.Extensions.DependencyInjection; // Provides dependency injection (DI) infrastructure
using Microsoft.Extensions.Hosting;          // Provides application lifecycle management capabilities
using Microsoft.Extensions.Logging;          // Provides infrastructure for logging
using Ocelot.DependencyInjection;            // Provides extensions for adding Ocelot services to the DI container
using Ocelot.Middleware;                     // Provides Ocelot middleware for routing requests
using Ocelot.Cache.CacheManager;             // Provides caching infrastructure for Ocelot

// Create a new ASP.NET Core application builder
// The args parameter contains command-line arguments that can influence application initialization
var builder = WebApplication.CreateBuilder(args);

#region Configuration Setup

// Configure configuration sources for the application
// Sets the base path where the system will look for configuration files
// ContentRootPath is the path where the application is running (not the location of static files)
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    // Load the base settings file appsettings.json
    // optional: false - the file must exist, otherwise an exception will be thrown
    // reloadOnChange: true - the application will detect changes to the file at runtime and reload them
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    // Load environment-specific settings file (e.g., appsettings.Development.json)
    // optional: true - if the file doesn't exist, the application will continue without it
    // EnvironmentName typically comes from the ASPNETCORE_ENVIRONMENT environment variable
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    // Load Ocelot's configuration file containing all routing settings
    // optional: false - the file must exist because it's essential for Gateway operation
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    // Add support for environment variables - allows overriding settings via environment variables
    // Especially important for container and cloud environments
    .AddEnvironmentVariables();

#endregion

#region Service Registration

// Add controllers support - part of the MVC architecture, without Views support
// Allows the use of controllers to manage API endpoints
builder.Services.AddControllers();

// Add a service that provides information about API endpoints
// Required for Swagger to generate automatic documentation
builder.Services.AddEndpointsApiExplorer();

// Add Swagger - automatic API documentation generator
// Allows testing and documenting endpoints through a graphical user interface
builder.Services.AddSwaggerGen();

// Add Ocelot services - the core of the API Gateway
// Provides all capabilities of routing, rate limiting, aggregation, etc.
builder.Services.AddOcelot(builder.Configuration)
    // Add caching capabilities to Ocelot
    // Improves performance by storing results of repeated queries
    .AddCacheManager(x =>
    {
        // Configure a memory-based Dictionary cache implementation
        // Simple and fast, but doesn't persist data between application restarts
        // and is not shared between different instances of the application
        x.WithDictionaryHandle();
    });

// Add CORS (Cross-Origin Resource Sharing) policy
// Necessary when clients from different domains need to access the API
builder.Services.AddCors(options =>
{
    // Define a policy named "CorsPolicy" that will be referenced later
    options.AddPolicy("CorsPolicy",
        policy => policy
            // Allow requests from any origin (domain)
            // In production, consider restricting to specific origins using .WithOrigins()
            .AllowAnyOrigin()
            // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
            // Can be restricted to specific methods using .WithMethods()
            .AllowAnyMethod()
            // Allow any HTTP headers in the request
            // Can be restricted to specific headers using .WithHeaders()
            .AllowAnyHeader());
});

#endregion

#region Application Building

// Build the application
// This finalizes the service registration and creates the application instance
var app = builder.Build();

// Configure the HTTP request pipeline based on environment
// Only enable developer-specific features in development environment
if (app.Environment.IsDevelopment())
{
    // Enable detailed exception page - shows stack traces and detailed error information
    // Should only be used in development to avoid exposing sensitive information
    app.UseDeveloperExceptionPage();

    // Enable Swagger middleware
    // Creates an endpoint at /swagger/v1/swagger.json that provides OpenAPI documentation
    app.UseSwagger();

    // Enable Swagger UI middleware
    // Creates an interactive page at /swagger for exploring and testing endpoints
    app.UseSwaggerUI();
}

// Enable automatic redirection from HTTP to HTTPS
// Enhances security by enforcing encrypted communication
app.UseHttpsRedirection();

// Enable the CORS policy defined earlier
// Adds appropriate headers to HTTP responses and handles preflight requests
app.UseCors("CorsPolicy");

// Enable authorization
// Required if using [Authorize] attribute in controllers or endpoints
// Must be placed after UseAuthentication() if authentication is used
app.UseAuthorization();

// Enable Ocelot middleware
// This is the core functionality that routes requests to the appropriate microservices
// based on the configuration in ocelot.json
// The await keyword is required because UseOcelot is an asynchronous method
await app.UseOcelot();

// Start the application
// This starts the internal HTTP server (Kestrel) and begins listening for HTTP requests
// Blocks the current thread until the application is terminated
app.Run();

#endregion