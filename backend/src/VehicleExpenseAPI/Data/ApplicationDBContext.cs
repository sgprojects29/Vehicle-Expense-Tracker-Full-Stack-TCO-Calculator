using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
  {
    
  }

  public DbSet<Vehicle> Vehicles { get; set; }
  public DbSet<Expense> Expenses { get; set; }
  public DbSet<FuelEntry> FuelEntries { get; set; }
  public DbSet<Receipt> Receipts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Currency fields decimnal precision
    modelBuilder.Entity<Vehicle>()
            .Property(v => v.PurchasePrice)
            .HasPrecision(18,2);

    modelBuilder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasPrecision(18, 2);

    modelBuilder.Entity<FuelEntry>()
            .Property(f => f.Cost)
            .HasPrecision(18, 2);

    modelBuilder.Entity<FuelEntry>()
            .Property(f => f.Amount)
            .HasPrecision(18, 3);

    modelBuilder.Entity<Receipt>()
            .Property(r => r.ParsedAmount)
            .HasPrecision(18, 2);

    // Build the relationships
    modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Vehicle)
            .WithMany()
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FuelEntry>()
            .HasOne(f => f.Vehicle)
            .WithMany()
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Receipt>()
            .HasOne(r => r.Expense)
            .WithOne()
            .HasForeignKey<Receipt>(r => r.ExpenseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}