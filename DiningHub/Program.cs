using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DiningHubContextConnection") ?? throw new InvalidOperationException("Connection string 'DiningHubContextConnection' not found.");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<DiningHubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DiningHubContextConnection")));

// Configure Identity
builder.Services.AddDefaultIdentity<DiningHubUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()  // Add roles support
    .AddEntityFrameworkStores<DiningHubContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Add roles to the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<DiningHubUser>>();

    await EnsureRolesAsync(roleManager);
    await EnsureTestAdminAsync(userManager);
}

async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
{
    var roles = new[] { "Manager", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

async Task EnsureTestAdminAsync(UserManager<DiningHubUser> userManager)
{
    var adminEmail = "admin@example.com";
    var adminPassword = "Admin@123";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new DiningHubUser
        {
            UserName = adminEmail,
            Email = adminEmail,
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Manager");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
