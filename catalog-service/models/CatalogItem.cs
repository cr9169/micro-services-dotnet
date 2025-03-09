using System.ComponentModel.DataAnnotations;

public class CatalogItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed from int to Guid with default value

    /// <summary>
    /// Using string.Empty as initial value instead of null:
    /// 1. Prevents null reference exceptions
    /// 2. More explicit and readable than ""
    /// 3. Better memory efficiency (reuses the same empty string instance)
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// The price of the catalog item.
    /// Using decimal instead of float because:
    /// - Decimal provides exact decimal arithmetic, crucial for financial calculationsאז 
    /// - Unlike float which can have rounding errors (e.g. 0.1 + 0.2 = 0.30000001192092896),
    ///   decimal always maintains precise decimal places (0.1 + 0.2 = 0.3 exactly)
    /// </summary>
    /// <example>
    /// decimal price = 10.99m;  // Will always be exactly 10.99
    /// decimal total = price * 3;  // Will be exactly 32.97
    /// </example>
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
}