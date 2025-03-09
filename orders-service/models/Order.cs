using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // Changed from int to Guid with default value

    [Required(ErrorMessage = "Customer Name  is required")]
    [StringLength(100, ErrorMessage = "Customer Name  cannot be longer than 100 characters")]
    public string CustomerName { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
}