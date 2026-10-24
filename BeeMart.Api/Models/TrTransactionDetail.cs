namespace BeeMart.Api.Models;

public class TrTransactionDetail
{
    public string IdTransactionDetail { get; set; } = null!;
    public string IdTransaction { get; set; } = null!;
    public string IdProduct { get; set; } = null!;
    public int Qty { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
    public bool Stsrc { get; set; }
    public DateTime Datein { get; set; }
    public string Userin { get; set; } = null!;
    public DateTime? Dateup { get; set; }
    public string? Userup { get; set; }

    public TrTransaction? Transaction { get; set; }
    public MsProduct? Product { get; set; }
}