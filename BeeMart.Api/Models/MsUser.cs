namespace BeeMart.Api.Models;

public class MsUser
{
    public string IdUser { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool Stsrc { get; set; }
    public DateTime Datein { get; set; }
    public string Userin { get; set; } = null!;
    public DateTime? Dateup { get; set; }
    public string? Userup { get; set; }
}