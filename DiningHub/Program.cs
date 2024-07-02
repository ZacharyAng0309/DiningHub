using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DiningHubContextConnection") ?? throw new InvalidOperationException("Connection string 'DiningHubContextConnection' not found.");

// Configure DbContext with SQL Server
builder.Services.AddDbContext<DiningHubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DiningHubContextConnection")));

// Configure Identity
builder.Services.AddDefaultIdentity<DiningHubUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()  // Add roles support
    .AddEntityFrameworkStores<DiningHubContext>();

// Secure cookies and authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add policy-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
    loggingBuilder.AddEventSourceLogger();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<DiningHubUser>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    await EnsureRolesAsync(roleManager, logger);
    await EnsureAdminAsync(userManager, logger);
}

async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
{
    var roles = new[] { "Manager", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (result.Succeeded)
            {
                logger.LogInformation($"Role '{role}' created successfully.");
            }
            else
            {
                logger.LogError($"Error creating role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}

async Task EnsureAdminAsync(UserManager<DiningHubUser> userManager, ILogger logger)
{
    var adminEmail = "admin@gmail.com";
    var adminPassword = "Admin@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new DiningHubUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true // Ensure email is confirmed at creation
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Manager");
            if (roleResult.Succeeded)
            {
                logger.LogInformation("Admin user created and added to Manager role successfully.");
            }
            else
            {
                logger.LogError($"Error adding admin user to Manager role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogError($"Error creating admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        logger.LogInformation("Admin user already exists.");
        /*
        logger.LogInformation("Resetting password.");
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        var resetResult = await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
        if (resetResult.Succeeded)
        {
            logger.LogInformation("Admin user password reset successfully.");
        }
        else
        {
            logger.LogError($"Error resetting admin user password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
        }
        */

        // Ensure the email is confirmed if not already
        if (!adminUser.EmailConfirmed)
        {
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(adminUser);
            var confirmResult = await userManager.ConfirmEmailAsync(adminUser, confirmationToken);
            if (confirmResult.Succeeded)
            {
                logger.LogInformation("Admin user's email confirmed successfully.");
            }
            else
            {
                logger.LogError($"Error confirming admin user's email: {string.Join(", ", confirmResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
