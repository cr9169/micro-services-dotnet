using System.ComponentModel.DataAnnotations;

public class OrderSharedDTO
{
    [StringLength(100, ErrorMessage = "Customer Name  cannot be longer than 100 characters")]
    public string? CustomerName { get; set; }
}