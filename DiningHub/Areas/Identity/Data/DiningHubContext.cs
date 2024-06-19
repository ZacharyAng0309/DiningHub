using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiningHub.Areas.Identity.Data;

public class DiningHubContext : IdentityDbContext<DiningHubUser>
{
    public DiningHubContext(DbContextOptions<DiningHubContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
