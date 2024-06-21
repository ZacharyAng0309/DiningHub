using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Areas.Identity.Data;

public class DiningHubContext : IdentityDbContext<DiningHubUser>
{
    public DiningHubContext(DbContextOptions<DiningHubContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // DiningHubUser and CustomerProfile relationship
        builder.Entity<DiningHubUser>()
            .HasOne(a => a.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<CustomerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // DiningHubUser and Orders relationship
        builder.Entity<DiningHubUser>()
            .HasMany(a => a.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Order and OrderItems relationship
        builder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order and Receipt relationship
        builder.Entity<Order>()
            .HasOne(o => o.Receipt)
            .WithOne(r => r.Order)
            .HasForeignKey<Receipt>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem and Menu relationship
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Menu)
            .WithMany(m => m.OrderItems)
            .HasForeignKey(oi => oi.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        // Feedback and DiningHubUser relationship
        builder.Entity<Feedback>()
            .HasOne(f => f.User)
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
