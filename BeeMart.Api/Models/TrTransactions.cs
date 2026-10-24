namespace BeeMart.Api.Models;

public class TrTransaction
{
    public string IdTransaction { get; set; } = null!;
    public string InvoiceNo { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string IdUser { get; set; } = null!;
    public int IdPaymentMethod { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Stsrc { get; set; }
    public DateTime Datein { get; set; }
    public string Userin { get; set; } = null!;
    public DateTime? Dateup { get; set; }
    public string? Userup { get; set; }

    public MsUser? User { get; set; }
    public LtPaymentMethod? PaymentMethod { get; set; }
    public List<TrTransactionDetail> Details { get; set; } = new();
}