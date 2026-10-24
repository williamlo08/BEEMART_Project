using BeeMart.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<LtPaymentMethod> LtPaymentMethods => Set<LtPaymentMethod>();
    public DbSet<MsUser> MsUsers => Set<MsUser>();
    public DbSet<MsProduct> MsProducts => Set<MsProduct>();
    public DbSet<TrTransaction> TrTransactions => Set<TrTransaction>();
    public DbSet<TrTransactionDetail> TrTransactionDetails => Set<TrTransactionDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LtPaymentMethod>(e =>
        {
            e.ToTable("LtPaymentMethod");
            e.HasKey(x => x.IdPaymentMethod);
            e.Property(x => x.MethodName).HasMaxLength(50);
            e.Property(x => x.Userin).HasMaxLength(36);
            e.Property(x => x.Userup).HasMaxLength(36);
        });

        modelBuilder.Entity<MsUser>(e =>
        {
            e.ToTable("MsUser");
            e.HasKey(x => x.IdUser);
            e.Property(x => x.IdUser).HasMaxLength(5);
            e.Property(x => x.Username).HasMaxLength(50);
            e.Property(x => x.FullName).HasMaxLength(300);
            e.Property(x => x.PasswordHash).HasMaxLength(255);
            e.Property(x => x.Role).HasMaxLength(20);
            e.Property(x => x.Userin).HasMaxLength(36);
            e.Property(x => x.Userup).HasMaxLength(36);
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<MsProduct>(e =>
        {
            e.ToTable("MsProduct");
            e.HasKey(x => x.IdProduct);
            e.Property(x => x.IdProduct).HasMaxLength(36);
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Weight).HasPrecision(10, 2);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(5, 2);
            e.Property(x => x.Userin).HasMaxLength(36);
            e.Property(x => x.Userup).HasMaxLength(36);
            e.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<TrTransaction>(e =>
        {
            e.ToTable("TrTransaction");
            e.HasKey(x => x.IdTransaction);
            e.Property(x => x.IdTransaction).HasMaxLength(36);
            e.Property(x => x.InvoiceNo).HasMaxLength(20);
            e.Property(x => x.IdUser).HasMaxLength(5);
            e.Property(x => x.TotalPrice).HasPrecision(18, 2);
            e.Property(x => x.Userin).HasMaxLength(36);
            e.Property(x => x.Userup).HasMaxLength(36);
            e.HasIndex(x => x.InvoiceNo).IsUnique();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.IdUser).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.PaymentMethod).WithMany().HasForeignKey(x => x.IdPaymentMethod).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TrTransactionDetail>(e =>
        {
            e.ToTable("TrTransactionDetail");
            e.HasKey(x => x.IdTransactionDetail);
            e.Property(x => x.IdTransactionDetail).HasMaxLength(36);
            e.Property(x => x.IdTransaction).HasMaxLength(36);
            e.Property(x => x.IdProduct).HasMaxLength(36);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(5, 2);
            e.Property(x => x.SubTotal).HasPrecision(18, 2);
            e.Property(x => x.Userin).HasMaxLength(36);
            e.Property(x => x.Userup).HasMaxLength(36);
            e.HasOne(x => x.Transaction).WithMany(t => t.Details).HasForeignKey(x => x.IdTransaction).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.IdProduct).OnDelete(DeleteBehavior.NoAction);
        });
    }
}