using System.ComponentModel.DataAnnotations;

public class OrderCreateDTO
{
    [Required]
    [StringLength(100, ErrorMessage = "Customer Name  cannot be longer than 100 characters")]
    public string CustomerName { get; set; } = string.Empty;
}