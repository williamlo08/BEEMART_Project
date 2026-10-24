using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PaymentMethodsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.LtPaymentMethods
            .Where(x => x.Stsrc)
            .OrderBy(x => x.IdPaymentMethod)
            .Select(x => new PaymentMethodDto
            {
                IdPaymentMethod = x.IdPaymentMethod,
                MethodName = x.MethodName
            })
            .ToListAsync();

        return Ok(list);
    }
}