using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.FullName) ||
            string.IsNullOrWhiteSpace(req.Password) ||
            string.IsNullOrWhiteSpace(req.RePassword) ||
            string.IsNullOrWhiteSpace(req.Role))
        {
            return BadRequest(new { message = "Semua field wajib diisi." });
        }

        
        if (req.Password != req.RePassword)
        {
            return BadRequest(new { message = "Password dan Re-Password tidak sama." });
        }

        
        var role = req.Role.Trim();
        if (role != "Admin" && role != "Kasir")
        {
            return BadRequest(new { message = "Role harus 'Admin' atau 'Kasir'." });
        }

        
        var exists = await _db.MsUsers.AnyAsync(u => u.Username == req.Username.Trim());
        if (exists)
        {
            return BadRequest(new { message = "username has already exists" });
        }

        
        var lastId = await _db.MsUsers
            .OrderByDescending(u => u.IdUser)
            .Select(u => u.IdUser)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (!string.IsNullOrEmpty(lastId) && lastId.StartsWith("U") && int.TryParse(lastId.Substring(1), out int lastNum))
        {
            nextNum = lastNum + 1;
        }
        var newIdUser = $"U{nextNum:D4}";  

        
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

        
        var user = new MsUser
        {
            IdUser = newIdUser,
            Username = req.Username.Trim(),
            FullName = req.FullName.Trim(),
            PasswordHash = hashedPassword,
            Role = role,
            Stsrc = true,
            Datein = DateTime.Now,
            Userin = "SYSTEM",
            Dateup = null,
            Userup = null
        };

        _db.MsUsers.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Registrasi berhasil.",
            idUser = user.IdUser,
            username = user.Username
        });
    }

    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Password) ||
            string.IsNullOrWhiteSpace(req.Role))
        {
            return BadRequest(new { message = "Username, Password, dan Role wajib diisi." });
        }

        var user = await _db.MsUsers
            .FirstOrDefaultAsync(u => u.Username == req.Username.Trim() && u.Stsrc);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid login" });
        }

        
        if (!string.Equals(user.Role, req.Role, StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "Invalid login" });
        }

        
        bool valid = false;
        try
        {
            valid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
        }
        catch
        {
            valid = false;
        }

        if (!valid)
        {
            return Unauthorized(new { message = "Invalid login" });
        }

        return Ok(new LoginResponse
        {
            IdUser = user.IdUser,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        });
    }
}