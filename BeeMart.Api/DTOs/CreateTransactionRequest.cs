namespace BeeMart.Api.DTOs;

public class CreateTransactionRequest
{
    // public string IdUser { get; set; } = null!; <-- HAPUS ATAU KOMENTARI BARIS INI
    public int IdPaymentMethod { get; set; }
    public List<CreateTransactionItem> Items { get; set; } = new();
}

public class CreateTransactionItem
{
    public string IdProduct { get; set; } = null!;
    public int Qty { get; set; }
}