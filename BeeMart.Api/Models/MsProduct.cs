namespace BeeMart.Api.Models;

public class MsProduct
{
    public string IdProduct { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Weight { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public bool Stsrc { get; set; }
    public DateTime Datein { get; set; }
    public string Userin { get; set; } = null!;
    public DateTime? Dateup { get; set; }
    public string? Userup { get; set; }
}