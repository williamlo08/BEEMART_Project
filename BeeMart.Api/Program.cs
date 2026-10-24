using BeeMart.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BeeMart API", Version = "v1" });
});

// DbContext — INI YANG WAJIB ADA agar AppDbContext bisa di-inject ke Controller
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// CORS untuk frontend
builder.Services.AddCors(o => o.AddPolicy("fe", p => p
    .WithOrigins(
        "http://localhost:5500",
        "http://127.0.0.1:5500",
        "http://localhost:3000",
        "http://127.0.0.1:3000"
    )
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeeMart API V1");
    });
}

app.UseCors("fe");
app.UseAuthorization();
app.MapControllers();
app.Run();
app.UseHttpsRedirection();