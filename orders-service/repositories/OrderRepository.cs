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

    public async Task<IEnumerable<Order>> GetAllAsync()
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

    public async Task<Order> GetByIdAsync(Guid id)
    {
        string cacheKey = $"order_{id}";

        if (_cache.TryGetValue(cacheKey, out Order? cachedOrder) && cachedOrder != null)
        {
            _logger.LogInformation("Returning order with ID {id} from cache", id);
            return cachedOrder;
        }

        try
        {

            var order = await _context.Orders.FindAsync(id);

            if (order is null)
            {
                _logger.LogWarning("Order with ID {id} was not found", id);
                throw new OrderNotFoundException(id);
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, order, cacheOptions);
            _logger.LogInformation("Cached order with ID {id} for 5 minutes", id);

            return order;
        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving order with ID {id}", id);
            throw new OrderRepositoryException($"Failed to retrieve order with ID {id}", ex);
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

    public async Task<Order> DeleteByIdAsync(Guid id)
    {
        try
        {
            var order = await GetByIdAsync(id);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            _cache.Remove($"order_{id}");
            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated cache for order with ID {id} and 'all_orders'", id);

            return order;

        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting order with ID {id}", id);
            throw new OrderRepositoryException($"Failed to delete order with ID {id}", ex);
        }
    }

    public async Task<Order> PatchByIdAsync(Guid id, OrderSharedDTO orderSharedDTO)
    {
        try
        {

            var existingOrder = await GetByIdAsync(id);

            if (orderSharedDTO.CustomerName != null)
                existingOrder.CustomerName = orderSharedDTO.CustomerName;

            await _context.SaveChangesAsync();

            _cache.Remove($"order_{id}");
            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated cache for order with ID {id} and 'all_orders'", id);

            return existingOrder;
        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while patching catalog item with ID {id}", id);
            throw new OrderRepositoryException($"Failed to patch catalog item with ID {id}", ex);
        }
    }

    public async Task<Order> UpdateByIdAsync(Guid id, OrderSharedDTO orderSharedDTO)
    {
        try
        {
            var existingOrder = await GetByIdAsync(id);

            if (orderSharedDTO.CustomerName != null)
                existingOrder.CustomerName = orderSharedDTO.CustomerName;

            _context.Orders.Update(existingOrder);
            await _context.SaveChangesAsync();

            _cache.Remove($"order_{id}");
            _cache.Remove("all_orders");
            _logger.LogInformation("Invalidated cache for order with ID {id} and 'all_orders'", id);

            return existingOrder;
        }
        catch (OrderNotFoundException)
        {
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error occurred while updating order with ID {id}", id);
            throw new OrderRepositoryException($"Failed to update order with ID {id}", ex);
        }
    }
}