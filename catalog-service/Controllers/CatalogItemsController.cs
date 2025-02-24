/// <summary>
/// Imports the Microsoft.AspNetCore.Mvc namespace, which provides core classes and attributes for building Web APIs in ASP.NET Core.
/// This includes essential types like ControllerBase, ActionResult, HttpGet, HttpPost, etc., used to define API endpoints and handle HTTP requests.
/// Without this, we couldn't use attributes like [ApiController] or methods like Ok() and NotFound() in this controller.
/// </summary>
using Microsoft.AspNetCore.Mvc;
/// <summary>
/// Imports the System.Net.Mime namespace, which provides constants and utilities for working with MIME types (e.g., "application/json").
/// This is used in attributes like [Produces] and [Consumes] to specify that the controller returns and accepts JSON data.
/// It ensures proper content negotiation and validation of HTTP request/response formats.
/// </summary>
using System.Net.Mime;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Specifies that this controller produces JSON responses.
/// MediaTypeNames.Application.Json resolves to "application/json"
/// This attribute ensures that all actions in this controller will return JSON-formatted responses.
/// It also helps with API documentation tools like Swagger/OpenAPI.
/// </summary>
[Produces(MediaTypeNames.Application.Json)]
/// <summary>
/// Specifies that this controller accepts JSON request bodies.
/// MediaTypeNames.Application.Json resolves to "application/json"
/// This attribute validates that incoming requests have the correct Content-Type header.
/// If a request with a different content type is received, it will automatically return a 415 Unsupported Media Type response.
/// </summary>
[Consumes(MediaTypeNames.Application.Json)]
public class CatalogItemsController : ControllerBase
{
    private readonly ICatalogItemRepository _repository;
    /* The CatalogItemsController type inside ILogger<T> tells the logging system that logs from this instance belong to CatalogItemsController.
    This helps categorize logs when filtering or searching in log files or cloud-based logging systems. */
    // Example of How Logging Appears in the Console: 
    // [2025-02-21 14:10:00] INFO  CatalogItemsController - Fetching all catalog items...
    // [2025-02-21 14:10:05] ERROR CatalogItemsController - Failed to fetch items from the database. 
    private readonly ILogger<CatalogItemsController> _logger;

    public CatalogItemsController(ICatalogItemRepository repository, ILogger<CatalogItemsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all catalog items
    /// </summary>
    /// <response code="200">Returns the list of catalog items</response>
    /// <response code="500">If there was an internal server error</response>
    /// <remarks>
    /// Note on the 'return' behavior: In this Web API context, the 'return' statements don't pass results to another part of the application code.
    /// Instead, they hand over an ActionResult to the ASP.NET Core framework, which constructs and sends an HTTP response to the client (e.g., browser or app).
    /// This is a key difference from traditional object-oriented programming, where 'return' typically passes data to the caller within the program.
    /// Here, the 'return' is more like a directive to the web server on how to respond to the HTTP request.
    /// </remarks>
    [HttpGet]
    /// <summary>
    /// Indicates that the action can return a 200 OK response.
    /// Returns: ActionResult<IEnumerable<CatalogItem>>
    /// The response body will contain a JSON array of catalog items.
    /// This is returned when the items are successfully retrieved from the database.
    /// </summary>
    [ProducesResponseType(typeof(IEnumerable<CatalogItem>), StatusCodes.Status200OK)]
    /// <summary>
    /// Indicates that the action can return a 500 Internal Server Error response.
    /// Returns: ProblemDetails object with error information
    /// This occurs when:
    /// - Database connection fails
    /// - Query execution errors
    /// - Unexpected server-side exceptions
    /// The response includes a general error message without exposing internal details.
    /// </summary>
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CatalogItem>>> GetAllItems()
    {
        try
        {
            // Logs an informational message to indicate the retrieval process has started.
            _logger.LogInformation("Retrieving all catalog items");
            // Calls the repository to fetch all catalog items asynchronously from the database.
            var items = await _repository.GetAllAsync();
            // Returns a 200 OK response with the list of items serialized as JSON.
            // The ActionResult is passed to ASP.NET Core, which sends it as an HTTP response.
            return Ok(items);
        }
        catch (CatalogItemRepositoryException ex)
        {
            // Catches exceptions specific to repository operations (e.g., database errors).
            // Logs the error with the exception details for debugging.
            _logger.LogError(ex, "Failed to retrieve all catalog items");
            // Returns a 500 Internal Server Error with a generic message.
            // The ActionResult is handed to ASP.NET Core to format and send the HTTP response.
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the catalog items." });
        }
    }

    /// <summary>
    /// Retrieves a specific catalog item by id
    /// </summary>
    /// <param name="id">The ID of the catalog item to retrieve</param>
    /// <response code="200">Returns the requested catalog item</response>
    /// <response code="404">If the catalog item was not found</response>
    /// <response code="500">If there was an internal server error</response>
    /// <remarks>
    /// Note on the 'return' behavior: The 'return' statements here instruct ASP.NET Core on how to respond to the HTTP request.
    /// They don't return data to another part of the application but instead define the HTTP response (status code and body) sent to the client.
    /// This aligns with the Web API paradigm, where the controller's role is to handle HTTP requests and define responses.
    /// </remarks>
    [HttpGet("{id}")]
    /// <summary>
    /// Indicates that the action can return a 200 OK response.
    /// Returns: ActionResult<CatalogItem>
    /// The response body will contain a single catalog item in JSON format.
    /// This is returned when the item with the specified ID exists and is successfully retrieved.
    /// </summary>
    [ProducesResponseType(typeof(CatalogItem), StatusCodes.Status200OK)]
    /// <summary>
    /// Indicates that the action can return a 404 Not Found response.
    /// Returns: ProblemDetails object with error message
    /// This occurs when:
    /// - The requested item ID doesn't exist in the database
    /// The response includes a message indicating the item was not found.
    /// </summary>
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    /// <summary>
    /// Indicates that the action can return a 500 Internal Server Error response.
    /// Returns: ProblemDetails object with error information
    /// This occurs when:
    /// - Database connection fails
    /// - Query execution errors
    /// - Unexpected server-side exceptions
    /// The response includes a general error message without exposing internal details.
    /// </summary>
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CatalogItem>> GetItemById(int id)
    {
        try
        {
            // Logs an informational message with the ID being retrieved for tracking.
            _logger.LogInformation("Retrieving catalog item with ID {Id}", id);
            // Attempts to fetch the item with the specified ID from the repository.
            var item = await _repository.GetByIdAsync(id);
            // Returns a 200 OK response with the item serialized as JSON if found.
            return Ok(item);
        }
        catch (CatalogItemNotFoundException ex)
        {
            // Catches an exception thrown when the item with the given ID doesn't exist.
            // Logs a warning with the exception details.
            _logger.LogWarning(ex, "Catalog item with ID {Id} was not found", id);
            // Returns a 404 Not Found response with a custom message from the exception.
            return NotFound(new { message = ex.Message });
        }
        catch (CatalogItemRepositoryException ex)
        {
            // Catches general repository exceptions (e.g., database connectivity issues).
            // Logs the error with full exception details.
            _logger.LogError(ex, "Failed to retrieve catalog item with ID {Id}", id);
            // Returns a 500 Internal Server Error with a generic error message.
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the catalog item." });
        }
    }

    /// <summary>
    /// Creates a new catalog item
    /// </summary>
    /// <param name="item">The catalog item to create</param>
    /// <response code="201">Returns the newly created catalog item</response>
    /// <response code="400">If the item data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    /// <remarks>
    /// Note on the 'return' behavior: The 'return' here defines the HTTP response sent to the client via ASP.NET Core.
    /// It doesn't pass data back to another method in the app but instructs the framework to send a 201 Created response (or an error) to the caller.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CatalogItem>> CreateItem([FromBody] CatalogItem item)
    {
        try
        {
            // Logs an informational message to indicate a new item creation is starting.
            _logger.LogInformation("Creating new catalog item");
            // Calls the repository to asynchronously create the item in the database.
            var createdItem = await _repository.CreateAsync(item);
            // Returns a 201 Created response with a location header pointing to GetItemById and the created item in the body.
            return CreatedAtAction(nameof(GetItemById), new { id = createdItem.Id }, createdItem);
        }
        catch (CatalogItemRepositoryException ex)
        {
            // Catches repository-specific exceptions (e.g., database insertion failure).
            // Logs the error with exception details.
            _logger.LogError(ex, "Failed to create catalog item");
            // Returns a 500 Internal Server Error with a generic message.
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the catalog item." });
        }
    }

    /// <summary>
    /// Updates a catalog item
    /// </summary>
    /// <param name="id">The ID of the catalog item to update</param>
    /// <param name="item">The updated catalog item data</param>
    /// <response code="200">Returns the updated catalog item</response>
    /// <response code="400">If the item data is invalid</response>
    /// <response code="404">If the catalog item was not found</response>
    /// <response code="500">If there was an internal server error</response>
    /// <remarks>
    /// Note on the 'return' behavior: The 'return' statements provide ASP.NET Core with instructions for the HTTP response.
    /// They don't return data to another part of the code but define what the client receives (e.g., 200 OK or 404 Not Found).
    /// </remarks>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CatalogItem>> UpdateItem(int id, [FromBody] CatalogItem item)
    {
        try
        {
            // Logs an informational message indicating an update is being attempted.
            _logger.LogInformation("Updating catalog item with ID {Id}", id);
            // Calls the repository to update the item with the given ID using the provided data.
            var updatedItem = await _repository.UpdateByIdAsync(id, item);
            // Returns a 200 OK response with the updated item in JSON format.
            return Ok(updatedItem);
        }
        catch (CatalogItemNotFoundException ex)
        {
            // Catches an exception when the item to update doesn't exist.
            // Logs a warning with the exception details.
            _logger.LogWarning(ex, "Catalog item with ID {Id} was not found", id);
            // Returns a 404 Not Found response with the exception message.
            return NotFound(new { message = ex.Message });
        }
        catch (CatalogItemRepositoryException ex)
        {
            // Catches general repository errors (e.g., database update failure).
            // Logs the error with full details.
            _logger.LogError(ex, "Failed to update catalog item with ID {Id}", id);
            // Returns a 500 Internal Server Error with a generic message.
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the catalog item." });
        }
    }

    /// <summary>
    /// Partially updates a catalog item
    /// </summary>
    /// <param name="id">The ID of the catalog item to update</param>
    /// <param name="patchDto">The partial update data</param>
    /// <response code="200">Returns the updated catalog item</response>
    /// <response code="400">If the patch data is invalid</response>
    /// <response code="404">If the catalog item was not found</response>
    /// <response code="500">If there was an internal server error</response>
    /// <remarks>
    /// Note on the 'return' behavior: The 'return' here tells ASP.NET Core how to format the HTTP response for the client.
    /// It's not passing data to another method but defining the outcome of this HTTP PATCH request.
    /// </remarks>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CatalogItem>> PatchItem(int id, [FromBody] CatalogItemPatchDTO patchDto)
    {
        try
        {
            // Logs an informational message about the partial update process.
            _logger.LogInformation("Patching catalog item with ID {Id}", id);
            // Calls the repository to apply a partial update to the item with the given ID.
            var patchedItem = await _repository.PatchByIdAsync(id, patchDto);
            // Returns a 200 OK response with the patched item in JSON format.
            return Ok(patchedItem);
        }
        catch (CatalogItemNotFoundException ex)
        {
            // Catches an exception when the item to patch doesn't exist.
            // Logs a warning with exception details.
            _logger.LogWarning(ex, "Catalog item with ID {Id} was not found", id);
            // Returns a 404 Not Found response with the exception message.
            return NotFound(new { message = ex.Message });
        }
        catch (CatalogItemRepositoryException ex)
        {
            // Catches general repository errors (e.g., issues applying the patch).
            // Logs the error with full details.
            _logger.LogError(ex, "Failed to patch catalog item with ID {Id}", id);
            // Returns a 500 Internal Server Error with a generic message.
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while patching the catalog item." });
        }
    }

    /// <summary>
    /// Deletes a catalog item
    /// </summary>
    /// <param name="id">The ID of the catalog item to delete</param>
    /// <response code="200">Returns the deleted catalog item</response>
    /// <response code="404">If the catalog item was not found</response>
    /// <response code="500">If there was an internal server error</response>
    /// <remarks>
    /// Note on the 'return' behavior: The 'return' statements direct ASP.NET Core to send an HTTP response to the client.
    /// They don't return data to another part of the application but define the result of the DELETE request (e.g., 200 OK or 404 Not Found).
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CatalogItem>> DeleteItem(int id)
    {
        try
        {
            // Logs an informational message about the deletion process.
            _logger.LogInformation("Deleting catalog item with ID {Id}", id);
            // Calls the repository to delete the item with the specified ID.
            var deletedItem = await _repository.DeleteByIdAsync(id);
            // Returns a 200 OK response with the deleted item in JSON format.
            return Ok(deletedItem);
        }
        catch (CatalogItemNotFoundException ex)
        {
            // Catches an exception when the item to delete doesn't exist.
            // Logs a warning with exception details.
            _logger.LogWarning(ex, "Catalog item with ID {Id} was not found", id);
            // Returns a 404 Not Found response with the exception message.
            return NotFound(new { message = ex.Message });
        }
        catch (CatalogItemRepositoryException ex)
        {
            // Catches general repository errors (e.g., database deletion failure).
            // Logs the error with full details.
            _logger.LogError(ex, "Failed to delete catalog item with ID {Id}", id);
            // Returns a 500 Internal Server Error with a generic message.
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the catalog item." });
        }
    }
}