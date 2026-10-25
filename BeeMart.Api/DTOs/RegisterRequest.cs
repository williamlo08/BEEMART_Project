namespace BeeMart.Api.DTOs;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RePassword { get; set; } = null!;
    public string Role { get; set; } = null!;  
}