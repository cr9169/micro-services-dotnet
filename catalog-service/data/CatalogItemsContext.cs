using Microsoft.EntityFrameworkCore;

// Inherits from DbContext, the base class in EF Core for managing database operations.
public class CatalogItemsContext : DbContext
{
    // DbSet<CatalogItem> means EF Core manages an "CatalogItems" table, allowing CRUD operations on Employee entities.
    public DbSet<CatalogItem> CatalogItems { get; set; }

    // Constructor for CatalogItemsContext that takes configuration options.
    // DbContextOptions<T> is a generic class that takes a DbContext type (T) and provides configuration options for it.
    // Here, DbContextOptions<CatalogItemsContext> ensures EF Core configures CatalogItemsContext with the correct database settings.
    // ": base(options)" - a call to the constructor of the base class, like super in java.
    public CatalogItemsContext(DbContextOptions<CatalogItemsContext> options) : base(options) {}
}