using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] int limit = 10)
    {
        if (limit <= 0) limit = 10;
        if (limit > 10) limit = 10;

        var q = _db.MsProducts.AsQueryable().Where(p => p.Stsrc);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search));

        var list = await q
            .OrderBy(p => p.Name)
            .Take(limit)
            .Select(p => new ProductDto
            {
                IdProduct = p.IdProduct,
                Name = p.Name,
                Weight = p.Weight,
                Stock = p.Stock,
                Price = p.Price,
                Discount = p.Discount
            })
            .ToListAsync();

        return Ok(list);
    }
}