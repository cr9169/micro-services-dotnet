public interface IOrderRepository
{
    Task<IEnumerable<Order>> getAllAsync();
    Task<Order> GetByIdAsync(Guid Id);
    Task<Order> CreateAsync(OrderCreateDTO orderSharedDTO);
    Task<Order> DeleteByIdAsync(Guid Id);
    Task<Order> UpdateByIdAsync(Guid Id, OrderSharedDTO orderSharedDTO);
    Task<Order> PatchByIdAsync(Guid Id, OrderSharedDTO orderSharedDTO);
}