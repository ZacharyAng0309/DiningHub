using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;
using System.Reflection.Emit;

namespace DiningHub.Areas.Identity.Data
{
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
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define relationships and additional configurations here if needed
            builder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.MenuItemId }); // Composite key

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Feedback)
                .WithOne(f => f.Order)
                .HasForeignKey<Feedback>(f => f.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete

            builder.Entity<MenuItem>()
                .Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<MenuItem>()
                .Property(m => m.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<InventoryItem>()
                .Property(i => i.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<InventoryItem>()
                .Property(i => i.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure foreign key constraints
            builder.Entity<InventoryItem>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryItem>()
                .HasOne(i => i.LastUpdatedBy)
                .WithMany()
                .HasForeignKey(i => i.LastUpdatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MenuItem>()
                .HasOne(m => m.CreatedBy)
                .WithMany()
                .HasForeignKey(m => m.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MenuItem>()
                .HasOne(m => m.LastUpdatedBy)
                .WithMany()
                .HasForeignKey(m => m.LastUpdatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Receipt>()
                .Property(r => r.TotalAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
