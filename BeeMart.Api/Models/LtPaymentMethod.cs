namespace BeeMart.Api.Models;

public class LtPaymentMethod
{
    public int IdPaymentMethod { get; set; }
    public string MethodName { get; set; } = null!;
    public bool Stsrc { get; set; }
    public DateTime Datein { get; set; }
    public string Userin { get; set; } = null!;
    public DateTime? Dateup { get; set; }
    public string? Userup { get; set; }
}