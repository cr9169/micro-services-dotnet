public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(OrderCreateDTO orderSharedDTO);
    Task<Order> DeleteByIdAsync(Guid id);
    Task<Order> UpdateByIdAsync(Guid id, OrderSharedDTO orderSharedDTO);
    Task<Order> PatchByIdAsync(Guid id, OrderSharedDTO orderSharedDTO);
}