/// <summary>
/// Centralized error handling middleware that wraps the entire request pipeline.
/// 
/// Key Concepts:
/// - Middleware in ASP.NET Core forms a nested pipeline of request processors
/// - Each middleware can perform actions before and after the next middleware
/// - Error handling middleware should be first to catch all possible errors
/// 
/// Pipeline Flow:
/// 1. Constructor runs once at startup:
///    - Receives next middleware delegate (_next)
///    - Receives logger for error tracking
/// 
/// 2. InvokeAsync runs for every request:
///    - Receives HttpContext (http context is like a req and res object in express) with request details.
///    - Tries to process the request through the pipeline
///    - Catches and handles any errors
/// 
/// 3. HandleExceptionAsync runs only for errors:
///    - Maps exceptions to HTTP status codes
///    - Formats consistent error responses
///    - Includes stack traces in development
/// 
/// Error Handling Strategy:
/// - Catches all unhandled exceptions
/// - Logs errors for monitoring
/// - Returns user-friendly error responses
/// - Hides technical details in production
/// - Maintains consistent error format
/// 
/// Integration Points:
/// - Works with dependency injection
/// - Integrates with logging system
/// - Respects environment settings
/// - Preserves HTTP content types
/// 
/// Usage:
/// Register in Program.cs with:
/// app.UseErrorHandling();
/// 
/// This ensures all requests are protected by error handling.
/// </summary>
public class ErrorHandlingMiddleware
{
    /// <summary>
    /// The _next delegate represents the next middleware component in the ASP.NET Core pipeline.
    /// It's a crucial part of the middleware chain pattern, allowing request processing to flow
    /// through multiple middleware components sequentially.
    /// 
    /// Key aspects:
    /// - private: Ensures the delegate can only be accessed within this middleware class
    /// - readonly: Guarantees the delegate cannot be reassigned after initialization
    /// - RequestDelegate: A delegate that can process an HTTP request
    /// 
    /// Usage:
    /// When invoked via await _next(context), the request continues to the next middleware
    /// in the pipeline. If _next is not called, the request processing stops at this middleware
    /// (short-circuiting the pipeline).
    /// 
    /// Similar concept in Express.js:
    /// function middleware(req, res, next) {
    ///     // next() in Express serves the same purpose as _next(context) in .NET
    ///     next();
    /// }
    /// 
    /// Technical details:
    /// RequestDelegate is defined as:
    /// public delegate Task RequestDelegate(HttpContext context);
    /// This means it's a function that takes an HttpContext and returns a Task.
    /// </summary>
    private readonly RequestDelegate _next;
    /* The ErrorHandlingMiddleware type inside ILogger<T> tells the logging system that logs from this instance belong to ErrorHandlingMiddleware.
    This helps categorize logs when filtering or searching in log files or cloud-based logging systems. */
    // Example of How Logging Appears in the Console: 
    // [2025-02-21 14:10:00] INFO  ErrorHandlingMiddleware - Fetching all catalog items...
    // [2025-02-21 14:10:05] ERROR ErrorHandlingMiddleware - Failed to fetch items from the database. 
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    /// <summary>
    /// Constructor for the middleware.
    /// RequestDelegate represents the next middleware in the pipeline (like next() in Express)
    /// </summary>
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Main middleware method that processes each request.
    /// This is similar to (req, res, next) in Express middleware.
    /// </summary>
    /// http context is like a req and res object in express.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass the request to the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Converts exceptions to appropriate HTTP responses.
    /// Maps different exception types to different status codes and formats.
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        // Map different exceptions to appropriate status codes
        // New syntax, the same as: 
        /*
        switch (ex)
        {
            case CatalogItemNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;
            case CatalogItemRepositoryException:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }
        */
        context.Response.StatusCode = ex switch
        {
            CatalogItemNotFoundException => StatusCodes.Status404NotFound,
            CatalogItemRepositoryException => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        // Create a consistent error response format
        var errorResponse = new
        {
            status = context.Response.StatusCode,
            message = ex.Message,
            // Add stack trace only in development
            detail = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                ? ex.StackTrace
                : null
        };

        // Converts the errorResponse (JSON object) to a JSON string,
        // and Writing it for the response.
        // "return" - stops the continuation of the execution, so that there are no further attempts to send a reply.
        return context.Response.WriteAsJsonAsync(errorResponse);
    }
}

/// <summary>
/// Extension method to make the middleware registration more elegant in Program.cs
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}