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

    public async Task<IEnumerable<Order>> getAllAsync()
    {
        string cacheKey = "all_orders";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Order>? cachedOrders) && cachedOrders != null)
        {
            _logger.LogInformation("Returning all orders from cache");
            return cachedOrders!;
        }

        try
        {
            var orders = await _context.Orders.ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, orders, cacheOptions);
            _logger.LogInformation("Cached all catalog orders for 5 minutes");

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all orders...");
            throw new OrderRepositoryException("Failed to retrieve orders", ex);
        }
    }

    public async Task<Order> GetByIdAsync(Guid Id)
    {
        string cacheKey = $"order_{Id}";

        if (_cache.TryGetValue(cacheKey, out Order? cachedOrder) && cachedOrder != null)
        {
            _logger.LogInformation("Returning order with ID {Id} from cache", Id);
            return cachedOrder;
        }

        try
        {

            var order = await _context.Orders.FindAsync(Id);

            if (order is null)
            {
                _logger.LogWarning("Order with ID {Id} was not found", Id);
                throw new OrderNotFoundException(Id);
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, order, cacheOptions);
            _logger.LogInformation("Cached order with ID {Id} for 5 minutes", Id);

            return order;
        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving order with ID {Id}", Id);
            throw new OrderRepositoryException($"Failed to retrieve order with ID {Id}", ex);
        }
    }

    public async Task<Order> CreateAsync(OrderCreateDTO orderCreateDTO)
    {
        try
        {

            var newOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = orderCreateDTO.CustomerName,
                OrderDate = DateTime.Now
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated 'all_orders' cache after creating new order");

            return newOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating order");
            throw new OrderRepositoryException("Failed to create order", ex);
        }
    }

    public async Task<Order> DeleteByIdAsync(Guid Id)
    {
        try
        {
            var order = await GetByIdAsync(Id);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _cache.Remove($"order_{Id}");
            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated cache for order with ID {Id} and 'all_orders'", Id);

            return order;

        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting order with ID {Id}", Id);
            throw new OrderRepositoryException($"Failed to delete order with ID {Id}", ex);
        }
    }

    public async Task<Order> PatchByIdAsync(Guid Id, OrderSharedDTO orderSharedDTO)
    {
        try
        {

            var existingOrder = await GetByIdAsync(Id);

            if (orderSharedDTO.CustomerName != null)
                existingOrder.CustomerName = orderSharedDTO.CustomerName;

            await _context.SaveChangesAsync();

            _cache.Remove($"order_{Id}");
            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated cache for order with ID {Id} and 'all_orders'", Id);

            return existingOrder;
        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while patching catalog item with ID {Id}", Id);
            throw new OrderRepositoryException($"Failed to patch catalog item with ID {Id}", ex);
        }
    }

    public async Task<Order> UpdateByIdAsync(Guid Id, OrderSharedDTO orderSharedDTO)
    {
        try
        {
            var existingOrder = await GetByIdAsync(Id);

            if (orderSharedDTO.CustomerName != null)
                existingOrder.CustomerName = orderSharedDTO.CustomerName;

            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();

            _cache.Remove($"order_{Id}");
            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated cache for order with ID {Id} and 'all_orders'", Id);

            return existingOrder;
        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while updating order with ID {Id}", Id);
            throw new OrderRepositoryException($"Failed to update order with ID {Id}", ex);
        }
    }
}