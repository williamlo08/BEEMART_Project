using BeeMart.Api.Data;
using BeeMart.Api.DTOs;
using BeeMart.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TransactionsController(AppDbContext db) => _db = db;

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest req)
    {
        if (req.Items == null || req.Items.Count == 0)
            return BadRequest(new { message = "Daftar item tidak boleh kosong." });

        var user = await _db.MsUsers.FirstOrDefaultAsync(u => u.IdUser == req.IdUser && u.Stsrc);
        if (user == null) return BadRequest(new { message = "User tidak ditemukan." });

        var pm = await _db.LtPaymentMethods.FirstOrDefaultAsync(p => p.IdPaymentMethod == req.IdPaymentMethod && p.Stsrc);
        if (pm == null) return BadRequest(new { message = "Metode pembayaran tidak valid." });

        // Ambil produk + validasi stock
        var productIds = req.Items.Select(i => i.IdProduct).Distinct().ToList();
        var products = await _db.MsProducts
            .Where(p => productIds.Contains(p.IdProduct) && p.Stsrc)
            .ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest(new { message = "Ada produk yang tidak ditemukan." });

        
        decimal total = 0;
        var details = new List<TrTransactionDetail>();
        var now = DateTime.Now;
        var transactionId = Guid.NewGuid().ToString();

        foreach (var item in req.Items)
        {
            if (item.Qty <= 0)
                return BadRequest(new { message = "Qty untuk produk harus > 0." });

            var p = products.First(x => x.IdProduct == item.IdProduct);

            if (item.Qty > p.Stock)
                return BadRequest(new { message = $"Stock {p.Name} tidak cukup (sisa {p.Stock})." });

            var subTotal = p.Price * item.Qty * (1 - p.Discount / 100m);

            details.Add(new TrTransactionDetail
            {
                IdTransactionDetail = Guid.NewGuid().ToString(),
                IdTransaction = transactionId,
                IdProduct = p.IdProduct,
                Qty = item.Qty,
                Price = p.Price,
                Discount = p.Discount,
                SubTotal = subTotal,
                Stsrc = true,
                Datein = now,
                Userin = req.IdUser
            });

            p.Stock -= item.Qty;
            p.Dateup = now;
            p.Userup = req.IdUser;

            total += subTotal;
        }

        
        var prefix = $"{req.IdUser}{now:ddMMyy}";
        var countToday = await _db.TrTransactions.CountAsync(t => t.InvoiceNo.StartsWith(prefix));
        var running = (countToday + 1).ToString("D3");
        var invoiceNo = $"{prefix}{running}";

        
        while (await _db.TrTransactions.AnyAsync(t => t.InvoiceNo == invoiceNo))
        {
            countToday++;
            invoiceNo = $"{prefix}{(countToday + 1).ToString("D3")}";
        }

        var trx = new TrTransaction
        {
            IdTransaction = transactionId,
            InvoiceNo = invoiceNo,
            TransactionDate = now,
            IdUser = req.IdUser,
            IdPaymentMethod = req.IdPaymentMethod,
            TotalPrice = total,
            Stsrc = true,
            Datein = now,
            Userin = req.IdUser,
            Details = details
        };

        _db.TrTransactions.Add(trx);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Transaksi berhasil.",
            idTransaction = trx.IdTransaction,
            invoiceNo = trx.InvoiceNo,
            totalPrice = trx.TotalPrice,
            transactionDate = trx.TransactionDate
        });
    }

        
    [HttpGet]
    public async Task<IActionResult> History(
        [FromQuery] string? date,
        [FromQuery] string? idUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 20) pageSize = 20;

        var q = _db.TrTransactions
            .Include(t => t.User)
            .Include(t => t.PaymentMethod)
            .Include(t => t.Details)
            .Where(t => t.Stsrc);

        
        if (!string.IsNullOrWhiteSpace(idUser))
        {
            q = q.Where(t => t.IdUser == idUser);
        }

        
        if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var dt))
        {
            var start = dt.Date;
            var end = start.AddDays(1);
            q = q.Where(t => t.TransactionDate >= start && t.TransactionDate < end);
        }

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionResponse
            {
                IdTransaction = t.IdTransaction,
                InvoiceNo = t.InvoiceNo,
                TransactionDate = t.TransactionDate,
                // LOGIKA BARU: Jika Role = Admin, tampilkan "Admin". Jika Kasir, tampilkan nama aslinya.
                Kasir = t.User != null && t.User.Role == "Admin" ? "Admin" : (t.User != null ? t.User.FullName : "-"),
                PaymentMethod = t.PaymentMethod != null ? t.PaymentMethod.MethodName : "-",
                TotalPrice = t.TotalPrice,
                TotalItems = t.Details.Sum(d => d.Qty)
            })
            .ToListAsync();

        var result = new PagedResult<TransactionResponse>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalData = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };

        return Ok(result);
    }
}