namespace BeeMart.Api.DTOs;

public class ProductDto
{
    public string IdProduct { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Weight { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}