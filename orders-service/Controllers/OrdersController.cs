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

    
}