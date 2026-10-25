using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize] // WAJIB LOGIN UNTUK AKSES ENDPOINT INI
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TransactionsController(AppDbContext db) => _db = db;

    // ============================================================
    // GET: Mendapatkan preview nomor invoice selanjutnya
    // (Agar Invoice muncul di layar sebelum transaksi dilakukan)
    // ============================================================
    [HttpGet("next-invoice")]
    public async Task<IActionResult> GetNextInvoice()
    {
        var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idUser == null) return Unauthorized();

        var now = DateTime.Now;
        var prefix = $"{idUser}{now:ddMMyy}";
        var countToday = await _db.TrTransactions.CountAsync(t => t.InvoiceNo.StartsWith(prefix));
        var invoiceNo = $"{prefix}{(countToday + 1).ToString("D3")}";

        return Ok(new { invoiceNo });
    }

    // ============================================================
    // POST: Simpan transaksi
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest req)
    {
        // Ambil IdUser dari Token JWT, bukan dari body request (Anti Curang)
        var idUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idUser == null) return Unauthorized();

        if (req.Items == null || req.Items.Count == 0)
            return BadRequest(new { message = "Keranjang belanja kosong." });

        using var dbTransaction = await _db.Database.BeginTransactionAsync();
        try
        {
            decimal total = 0;
            var details = new List<TrTransactionDetail>();
            var now = DateTime.Now;
            var trxId = Guid.NewGuid().ToString();

            foreach (var item in req.Items)
            {
                var p = await _db.MsProducts.FirstOrDefaultAsync(x => x.IdProduct == item.IdProduct && x.Stsrc);
                if (p == null) return BadRequest(new { message = "Produk tidak ditemukan." });

                // Cek stok (Atomic Lock via Transaction)
                if (p.Stock < item.Qty) 
                    return BadRequest(new { message = $"Stok {p.Name} tidak mencukupi." });

                p.Stock -= item.Qty; // Kurangi stok

                var subTotal = p.Price * item.Qty * (1 - p.Discount / 100m);
                details.Add(new TrTransactionDetail
                {
                    IdTransactionDetail = Guid.NewGuid().ToString(),
                    IdTransaction = trxId,
                    IdProduct = p.IdProduct,
                    Qty = item.Qty,
                    Price = p.Price,
                    Discount = p.Discount,
                    SubTotal = subTotal,
                    Stsrc = true, Datein = now, Userin = idUser
                });
                total += subTotal;
            }

            // FORMAT INVOICE: [5 digit userID][ddmmyy][3 digit running number]
            var prefix = $"{idUser}{now:ddMMyy}";
            var countToday = await _db.TrTransactions.CountAsync(t => t.InvoiceNo.StartsWith(prefix));
            var invoiceNo = $"{prefix}{(countToday + 1).ToString("D3")}";

            var trx = new TrTransaction
            {
                IdTransaction = trxId,
                InvoiceNo = invoiceNo,
                TransactionDate = now,
                IdUser = idUser,
                IdPaymentMethod = req.IdPaymentMethod,
                TotalPrice = total,
                Stsrc = true, Datein = now, Userin = idUser,
                Details = details
            };

            _db.TrTransactions.Add(trx);
            await _db.SaveChangesAsync();
            
            await dbTransaction.CommitAsync();

            return Ok(new { message = "Transaksi berhasil.", invoiceNo = trx.InvoiceNo, totalPrice = trx.TotalPrice });
        }
        catch (Exception)
        {
            await dbTransaction.RollbackAsync();
            return StatusCode(500, new { message = "Terjadi kesalahan sistem saat memproses transaksi." });
        }
    }

    // ============================================================
    // GET: History Transaksi (dengan Pagination)
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> History([FromQuery] string? date, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 20) pageSize = 20;

        var role = User.FindFirstValue(ClaimTypes.Role);
        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var q = _db.TrTransactions
            .Include(t => t.User).Include(t => t.PaymentMethod).Include(t => t.Details)
            .Where(t => t.Stsrc);

        // RESTRICTION: Jika yang login Cashier, filter hanya milik dia
        if (role == "Cashier")
        {
            q = q.Where(t => t.IdUser == myId);
        }

        // Filter by date
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
        {
            var start = dt.Date;
            var end = start.AddDays(1);
            q = q.Where(t => t.TransactionDate >= start && t.TransactionDate < end);
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new TransactionResponse
            {
                IdTransaction = t.IdTransaction,
                InvoiceNo = t.InvoiceNo,
                TransactionDate = t.TransactionDate,
                // Admin ditampilkan "Admin", Cashier ditampilkan namanya
                Kasir = t.User != null && t.User.Role == "Admin" ? "Admin" : (t.User != null ? t.User.Username : "-"),
                PaymentMethod = t.PaymentMethod != null ? t.PaymentMethod.MethodName : "-",
                TotalPrice = t.TotalPrice,
                TotalItems = t.Details.Sum(d => d.Qty)
            }).ToListAsync();

        return Ok(new PagedResult<TransactionResponse>
        {
            Items = items, 
            CurrentPage = page, 
            PageSize = pageSize, 
            TotalData = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }
}