namespace BeeMart.Api.DTOs;

public class TransactionResponse
{
    public string IdTransaction { get; set; } = null!;
    public string InvoiceNo { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string Kasir { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPages { get; set; }
}