using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
public class OrdersController : ControllerBase
{

    private readonly IOrderRepository _repository;
    private readonly ILogger<OrdersController> _logger;
    private OrdersController(IOrderRepository repository, ILogger<OrdersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
    {
        try
        {
            _logger.LogInformation("Retrieving all orders");
            var orders = await _repository.GetAllAsync();
            return Ok(orders);
        }
        catch (OrderRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to retrieve all orders");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the orders." });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Order>> GetOrderById(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving order with ID {id}", id);
            var order = await _repository.GetByIdAsync(id);
            return Ok(order);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {id} was not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (OrderRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to retrieve order with ID {id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the order." });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] OrderCreateDTO order)
    {
        try
        {
            _logger.LogInformation("Creating new order");
            var createdOrder = await _repository.CreateAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
        }
        catch (OrderRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to create order");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the order." });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Order>> UpdateOrder(Guid id, [FromBody] OrderSharedDTO orderSharedDTO)
    {
        try
        {
            _logger.LogInformation("Updating order with ID {id}", id);
            var updatedOrder = await _repository.UpdateByIdAsync(id, orderSharedDTO);
            return Ok(updatedOrder);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {id} was not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (OrderRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to update order with ID {id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the order." });
        }
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Order>> PatchOrder(Guid id, [FromBody] OrderSharedDTO orderSharedDTO)
    {
        try
        {
            _logger.LogInformation("Patching order with ID {id}", id);
            var patchedOrder = await _repository.PatchByIdAsync(id, orderSharedDTO);
            return Ok(patchedOrder);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {id} was not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (OrderRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to patch order with ID {id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while patching the order." });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Order>> DeleteOrder(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting order with ID {id}", id);
            var deletedOrder = await _repository.DeleteByIdAsync(id);
            return Ok(deletedOrder);
        }
        catch (OrderNotFoundException ex)
        {
            _logger.LogWarning(ex, "Order with ID {id} was not found", id);
            return NotFound(new { message = ex.Message });
        }
        catch (OrderRepositoryException ex)
        {
            _logger.LogError(ex, "Failed to delete order with ID {id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while deleting the order." });
        }
    }
}