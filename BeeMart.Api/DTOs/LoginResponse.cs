namespace BeeMart.Api.DTOs;

public class LoginResponse
{
    public string IdUser { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
}