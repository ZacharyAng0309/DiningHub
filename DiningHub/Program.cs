using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DiningHub.Data; // Add the correct namespace for SeedData
// using Amazon.S3; // Uncomment for AWS S3 integration
// using Amazon.Extensions.NETCore.Setup; // Uncomment for AWS S3 integration

var builder = WebApplication.CreateBuilder(args);
string connectionString;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DiningHubContextConnection") ?? throw new InvalidOperationException("Connection string 'DiningHubContextConnection' not found.");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DiningHubContextConnectionCloud") ?? throw new InvalidOperationException("Connection string 'DiningHubContextConnectionCloud' not found.");
}

// Configure DbContext with SQL Server
builder.Services.AddDbContext<DiningHubContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity
builder.Services.AddDefaultIdentity<DiningHubUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
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
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
    options.AddPolicy("RequireInternalRole", policy => policy.RequireRole("Manager", "Staff"));
    options.AddPolicy("RequireAnyRole", policy => policy.RequireRole("Manager", "Customer", "Staff"));
});

// Uncomment these lines for AWS S3 integration
/*
builder.Services.AddAWSService<IAmazonS3>(new AWSOptions
{
    Credentials = new BasicAWSCredentials(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"]
    ),
    Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"])
});
*/

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

    await SeedData.InitializeAsync(services);
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
