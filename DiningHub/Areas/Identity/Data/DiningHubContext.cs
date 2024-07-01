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
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Define relationships and additional configurations here if needed
        builder.Entity<OrderItem>()
            .HasKey(oi => new { oi.OrderId, oi.MenuItemId });

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId);
    }
}