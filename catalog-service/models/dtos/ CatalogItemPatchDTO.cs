using System.ComponentModel.DataAnnotations;

public class CatalogItemPatchDTO
{
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
    public string? Description { get; set; }

    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? Price { get; set; }
}