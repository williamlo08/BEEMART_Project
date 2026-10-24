namespace BeeMart.Api.DTOs;

public class PaymentMethodDto
{
    public int IdPaymentMethod { get; set; }
    public string MethodName { get; set; } = null!;
}