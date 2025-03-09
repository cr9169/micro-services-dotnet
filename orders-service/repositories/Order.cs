using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class OrderRepository : IOrderRepository
{

    private readonly OrdersContext _context;
    private readonly ILogger<OrderRepository> _logger;
    private readonly IMemoryCache _cache;

    public OrderRepository(OrdersContext context, ILogger<OrderRepository> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public Task<IEnumerable<Order>> getAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Order> GetByIdAsync(Guid Id)
    {
        throw new NotImplementedException();
    }

    public Task<Order> CreateAsync(OrderSharedDTO orderSharedDTO)
    {
        throw new NotImplementedException();
    }

    public Task<Order> DeleteByIdAsync(Guid Id)
    {
        throw new NotImplementedException();
    }

    public Task<Order> PatchByIdAsync(Guid Id, OrderSharedDTO orderSharedDTO)
    {
        throw new NotImplementedException();
    }

    public Task<Order> UpdateByIdAsync(Guid Id, OrderSharedDTO orderSharedDTO)
    {
        throw new NotImplementedException();
    }
}