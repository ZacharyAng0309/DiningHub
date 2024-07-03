using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;

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
        public DbSet<Receipt> Receipts { get; set; }

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

            builder.Entity<Order>()
                .HasOne(o => o.Feedback)
                .WithOne(f => f.Order)
                .HasForeignKey<Feedback>(f => f.OrderId)
                .OnDelete(DeleteBehavior.NoAction); // Change cascade delete behavior here

            builder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Keep cascade delete for User
        }
    }
}
