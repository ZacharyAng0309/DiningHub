using DiningHub.Areas.Identity.Data;
using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using DiningHub.Helper;

namespace DiningHub.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<DiningHubContext>())
            {
                // Ensure the database is created
                context.Database.EnsureCreated();

                // Seed initial roles
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await EnsureRolesAsync(roleManager);

                // Seed initial admin user
                var userManager = serviceProvider.GetRequiredService<UserManager<DiningHubUser>>();
                var adminUser = await EnsureAdminAsync(userManager);

                // Seed staff and customer users
                var staffUser = await EnsureStaffAsync(userManager);
                var customerUser = await EnsureCustomerAsync(userManager);

                // Seed initial categories if not already seeded
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Appetizers" },
                        new Category { Name = "Main Courses" },
                        new Category { Name = "Cheeses" },
                        new Category { Name = "Desserts" },
                        new Category { Name = "Beverages" }


                    );
                    await context.SaveChangesAsync();
                }

                // Fetch categories
                var appetizersCategory = context.Categories.First(c => c.Name == "Appetizers").CategoryId;
                var mainCoursesCategory = context.Categories.First(c => c.Name == "Main Courses").CategoryId;
                var cheeseCategory = context.Categories.First(c => c.Name == "Cheeses").CategoryId;
                var dessertsCategory = context.Categories.First(c => c.Name == "Desserts").CategoryId;
                var beveragesCategory = context.Categories.First(c => c.Name == "Beverages").CategoryId;

                // Seed initial menu items if not already seeded
                if (!context.MenuItems.Any())
                {
                    context.MenuItems.AddRange(
                        new MenuItem
                        {
                            Name = "French Onion Soup",
                            Description = "Classic French onion soup with caramelized onions, beef broth, and a cheesy crouton topping.",
                            Price = 28.00m,
                            CategoryId = appetizersCategory,
                            ImageUrl = "/Img/menu/appetizers1.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Escargots de Bourgogne",
                            Description = "Burgundy snails cooked in garlic butter and herbs, served with fresh baguette slices.",
                            Price = 38.00m,
                            CategoryId = appetizersCategory,
                            ImageUrl = "/Img/menu/appetizers2.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Foie Gras",
                            Description = "Pan-seared foie gras served with a fig compote and toasted brioche.",
                            Price = 68.00m,
                            CategoryId = appetizersCategory,
                            ImageUrl = "/Img/menu/appetizers3.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Salade Nicoise",
                            Description = "Mixed greens with tuna, hard-boiled eggs, green beans, potatoes, olives, and anchovies, dressed with vinaigrette.",
                            Price = 32.00m,
                            CategoryId = appetizersCategory,
                            ImageUrl = "/Img/menu/appetizers4.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Coq au Vin",
                            Description = "Chicken braised with red wine, mushrooms, bacon, and pearl onions, served with mashed potatoes.",
                            Price = 68.00m,
                            CategoryId = mainCoursesCategory,
                            ImageUrl = "/Img/menu/maincourse1.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Boeuf Bourguignon",
                            Description = "Slow-cooked beef stew with red wine, carrots, onions, and mushrooms, served with a side of crusty bread.",
                            Price = 75.00m,
                            CategoryId = mainCoursesCategory,
                            ImageUrl = "/Img/menu/maincourse2.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Sole Meuniere",
                            Description = "Sole fish lightly floured and pan-fried in butter, lemon, and parsley, served with steamed vegetables.",
                            Price = 85.00m,
                            CategoryId = mainCoursesCategory,
                            ImageUrl = "/Img/menu/maincourse3.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Ratatouille",
                            Description = "A Provençal vegetable dish made with layers of zucchini, eggplant, tomatoes, and bell peppers, served with couscous.",
                            Price = 45.00m,
                            CategoryId = mainCoursesCategory,
                            ImageUrl = "/Img/menu/maincourse4.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Selection of French Cheeses",
                            Description = "An assortment of French cheeses, including Brie, Camembert served with fresh baguette slices, crackers, fruit, nuts, honey, and jams.",
                            Price = 98.00m,
                            CategoryId = cheeseCategory,
                            ImageUrl = "/Img/menu/cheese1.png",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Creme Brulee",
                            Description = "Vanilla custard topped with a caramelized sugar crust.",
                            Price = 25.00m,
                            CategoryId = dessertsCategory,
                            ImageUrl = "/Img/menu/dessert1.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Tarte Tatin",
                            Description = "Caramelized apple tart served warm with a scoop of vanilla ice cream.",
                            Price = 28.00m,
                            CategoryId = dessertsCategory,
                            ImageUrl = "/Img/menu/dessert2.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Wine Selection",
                            Description = "A carefully curated list of French wines, including red, white, and sparkling options.",
                            Price = 40.00m,
                            CategoryId = beveragesCategory,
                            ImageUrl = "/Img/menu/drinks1.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Cafe",
                            Description = "Traditional French coffee, including espresso, cappuccino, and café au lait.",
                            Price = 15.00m,
                            CategoryId = beveragesCategory,
                            ImageUrl = "/Img/menu/drinks2.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        }
                    );
                    context.SaveChanges();
                }

                // Seed initial inventory items if not already seeded
                if (!context.InventoryItems.Any())
                {
                    context.InventoryItems.AddRange(
                        new InventoryItem
                        {
                            Name = "Garlic",
                            Description = "Used in Escargots de Bourgogne and various sauces.",
                            Quantity = 100,
                            CategoryId = appetizersCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Onions",
                            Description = "Essential for Soupe à l'oignon and flavoring various appetizers.",
                            Quantity = 50,
                            CategoryId = appetizersCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Baguette",
                            Description = "Served with Escargots de Bourgogne and cheese selections.",
                            Quantity = 200,
                            CategoryId = appetizersCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Butter",
                            Description = "Used for sautéing, making sauces, and enriching dishes like Coq au Vin and Sole Meunière.",
                            Quantity = 30,
                            CategoryId = mainCoursesCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Red Wine",
                            Description = "Key ingredient for braising in dishes like Coq au Vin and Boeuf Bourguignon.",
                            Quantity = 50,
                            CategoryId = mainCoursesCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Thyme",
                            Description = "A commonly used herb in stews like Boeuf Bourguignon and Coq au Vin.",
                            Quantity = 25,
                            CategoryId = mainCoursesCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Cheese",
                            Description = "Varieties like Brie, Camembert, and Roquefort are served in the cheese selection.",
                            Quantity = 40,
                            CategoryId = cheeseCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Heavy Cream",
                            Description = "Used in making Crème Brûlée and other creamy desserts.",
                            Quantity = 60,
                            CategoryId = dessertsCategory,
                            CreatedById = adminUser.Id,
                            LastUpdatedById = adminUser.Id
                        }
                    );
                    context.SaveChanges();
                }
            }
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Manager", "Customer", "Staff" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task<DiningHubUser> EnsureAdminAsync(UserManager<DiningHubUser> userManager)
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
                    PhoneNumber = "0123456789",
                    DateOfBirth = DateTimeHelper.GetMalaysiaTime().AddYears(-30), // Optional DateOfBirth
                    EmailConfirmed = true // Ensure email is confirmed at creation
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Manager");
                }
            }
            return adminUser;
        }

        private static async Task<DiningHubUser> EnsureStaffAsync(UserManager<DiningHubUser> userManager)
        {
            var staffEmail = "staff@gmail.com";
            var staffPassword = "Staff@123";

            var staffUser = await userManager.FindByEmailAsync(staffEmail);
            if (staffUser == null)
            {
                staffUser = new DiningHubUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    FirstName = "Staff",
                    LastName = "Member",
                    PhoneNumber = "0123456789",
                    DateOfBirth = DateTimeHelper.GetMalaysiaTime().AddYears(-25), // Optional DateOfBirth
                    EmailConfirmed = true // Ensure email is confirmed at creation
                };

                var result = await userManager.CreateAsync(staffUser, staffPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                }
            }
            return staffUser;
        }

        private static async Task<DiningHubUser> EnsureCustomerAsync(UserManager<DiningHubUser> userManager)
        {
            var customerEmail = "customer@gmail.com";
            var customerPassword = "Customer@123";

            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new DiningHubUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "Customer",
                    LastName = "User",
                    PhoneNumber = "0123456789",
                    DateOfBirth = DateTimeHelper.GetMalaysiaTime().AddYears(-20), // Optional DateOfBirth
                    EmailConfirmed = true // Ensure email is confirmed at creation
                };

                var result = await userManager.CreateAsync(customerUser, customerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
            return customerUser;
        }
    }
}
