using Microsoft.EntityFrameworkCore;

public class OrdersContext : DbContext
{

    public DbSet<Order> Orders { get; set; }
    OrdersContext(DbContextOptions<OrdersContext> options) : base(options) { }

}