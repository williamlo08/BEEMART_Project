using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    
    public AuthController(AppDbContext db, IConfiguration config) 
    { 
        _db = db; 
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password) || req.Password != req.RePassword)
            return BadRequest(new { message = "Data tidak valid atau password tidak sama." });

        // Batasi role hanya Admin dan Cashier
        var role = req.Role.Trim();
        if (role != "Admin" && role != "Cashier")
            return BadRequest(new { message = "Role tidak valid." });

        var exists = await _db.MsUsers.AnyAsync(u => u.Username == req.Username.Trim());
        if (exists) return BadRequest(new { message = "username has already exists" });

        var lastId = await _db.MsUsers.OrderByDescending(u => u.IdUser).Select(u => u.IdUser).FirstOrDefaultAsync();
        int nextNum = 1;
        if (!string.IsNullOrEmpty(lastId) && lastId.StartsWith("U") && int.TryParse(lastId.Substring(1), out int lastNum))
            nextNum = lastNum + 1;

        var newIdUser = $"U{nextNum:D4}";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var user = new MsUser
        {
            IdUser = newIdUser,
            Username = req.Username.Trim(),
            FullName = req.FullName.Trim(),
            PasswordHash = hashedPassword,
            Role = role,
            Stsrc = true, Datein = DateTime.Now, Userin = "SYSTEM"
        };

        _db.MsUsers.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Registrasi berhasil.", idUser = user.IdUser });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.MsUsers.FirstOrDefaultAsync(u => u.Username == req.Username.Trim() && u.Stsrc);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid login" });

        if (!string.Equals(user.Role, req.Role, StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { message = "Role tidak cocok dengan akun Anda." });

        // GENERATE JWT TOKEN
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUser),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName)
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new
        {
            idUser = user.IdUser,
            fullName = user.FullName,
            username = user.Username,
            role = user.Role,
            token = tokenHandler.WriteToken(token)
        });
    }
}