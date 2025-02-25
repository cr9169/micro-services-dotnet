using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// The first step in creating an ASP.NET Core application. The builder provides the basic infrastructure for the application and contains all the services and configuration.
var builder = WebApplication.CreateBuilder(args);

/* 
1. Finds the .env file in the project folder
2. Reads the file
3. Breaks each line into a key and value
4. Adds each pair as an environment variable to the system (in the memory of the current process) 
*/
Env.Load();

// Adds all environment variables to the application configuration system. This allows you to access them via builder.Configuration.
builder.Configuration.AddEnvironmentVariables();


// alternative - using the data from the appsettings.json "ConnectionStrings.CatalogConnection" object.
/* builder.Services.AddDbContext<CatalogItemsContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogConnection"))); */
// db contect is like a mongoose connection to mongo.
builder.Services.AddDbContext<CatalogItemsContext>(options =>
options.UseSqlServer(builder.Configuration["DB_CONNECTION_URL"]));

/* Doesn't have to create a constructor in this case in AppConfig. We are "Object Initializer Syntax",
   it's a shorthand and convenient way to create and initialize an object. */
var appConfig = new AppConfig
{
    DbConnectionUrl = builder.Configuration["DB_CONNECTION_URL"],
    JwtSecret = builder.Configuration["JWT_SECRET"] ?? "DefaultSecretKey",
};


// Registering an AppConfig instance as a Singleton instance, so it can be injected anywhere in the system.
/* That means: 
   1. Take the appConfig object.
   2. Register it in the app's DI container.
   3. Every time someone requests AppConfig, return the exact same instance. */
builder.Services.AddSingleton(appConfig);

// AddScoped adds a service, the CatalogItemRepository to the DI system for the current life time (Scoped).
// This means that for every HTTP request, it will create a new instance of the CatalogItemRepository.
// This means every time ICatalogItemRepository is requested, DI creates an CatalogItemRepository instance.
builder.Services.AddScoped<ICatalogItemRepository, CatalogItemRepository>();

builder.Services.AddControllers();

// Register IMemoryCache in the app's DI container.
builder.Services.AddMemoryCache();

/// <summary>
/// Health Check Configuration
/// This section configures health monitoring for the service and its dependencies.
/// 
/// AddHealthChecks(): Registers health check services
/// AddDbContextCheck<CatalogItemsContext>(): Adds a health check for the database connection
/// 
/// The health check endpoint ("/health") will return:
/// - 200 OK: Service and database are healthy
/// - 503 Service Unavailable: Issues with service or database connection
/// 
/// Usage:
/// - GET /health - Returns the health status
/// - Useful for monitoring systems and container orchestrators
/// </summary>
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CatalogItemsContext>(
        // Name of the health check
        name: "database",
        // Status when check fails.
        /*
        public enum HealthStatus
        {
            Unhealthy = 0,   // The service is not working properly.
            Degraded = 1,    // The service works but with poor performance.
            Healthy = 2      // The service works as it should.
        }
        */
        failureStatus: HealthStatus.Unhealthy,
        // "ready" - Indicates that this is a basic readiness test of the service.
        // "db" - Indicates that this is a test related to the database.
        tags: ["ready", "db"]
    );

// Register Swagger
// find all the methods in the controllers to create a configuration file for the API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/// <summary>
/// Registers the ErrorHandlingMiddleware in the application's middleware pipeline.
/// 
/// Detailed Flow:
/// 1. When this line executes during application startup:
///    - ASP.NET Core creates a new instance of ErrorHandlingMiddleware
///    - The system injects dependencies (next middleware delegate and logger)
///    - The middleware is added to the beginning of the request pipeline
/// 
/// 2. For each incoming HTTP request:
///    - Request arrives at the server
///    - ErrorHandlingMiddleware's InvokeAsync is automatically called first
///    - The request flows through the try/catch block
///    - If successful, request continues to other middleware
///    - If an exception occurs, HandleExceptionAsync formats an error response
/// 
/// Understanding the Chain:
/// - Think of middleware like security checkpoints at an airport
/// - Each piece of middleware is a checkpoint
/// - UseErrorHandling() puts our error handler at the first checkpoint
/// - This ensures it can catch errors from all later checkpoints
/// 
/// Technical Details:
/// - The extension method UseErrorHandling() comes from ErrorHandlingMiddlewareExtensions
/// - It calls builder.UseMiddleware<ErrorHandlingMiddleware>() internally
/// - The system uses reflection to find and call InvokeAsync for each request
/// - The middleware pattern allows for clean separation of cross-cutting concerns
/// 
/// Example Flow:
/// 1. Request comes in to /api/catalog/items
/// 2. ErrorHandlingMiddleware.InvokeAsync starts
/// 3. Try block begins
/// 4. Request passes to next middleware (controllers, etc.)
/// 5. If an error occurs anywhere:
///    - Catch block triggers
///    - Error is logged
///    - Formatted error response is sent back
/// 6. If no error:
///    - Response flows back through middleware chain
///    - Client gets successful response
/// </summary>
app.UseErrorHandling();

// Checks if the application is running in the development environment.
if (app.Environment.IsDevelopment())
{
    // Enables Swagger middleware to generate the API documentation.
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "swagger/catalog/service/{documentName}/swagger.json";
    });

    // Configures the Swagger UI for interactive API documentation.
    app.UseSwaggerUI(c =>
    {
        // Defines the Swagger JSON file location and sets the API title.
        c.SwaggerEndpoint("/swagger/catalog/service/v1/swagger.json", "Catalog Service API - V1");
        // Sets Swagger UI as the default page by removing the /swagger prefix.
        c.RoutePrefix = string.Empty;
    });
}

// Maps all controller routes to handle HTTP requests in the application.
// This ensures that API endpoints defined in controllers (marked with [ApiController]) are correctly registered and accessible.
app.MapControllers();


// Add the health check endpoint
app.MapHealthChecks("/health");

app.Run();
