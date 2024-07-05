﻿using DiningHub.Areas.Identity.Data;
using DiningHub.Models;
using Microsoft.AspNetCore.Identity;

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

                // Seed initial menu items if not already seeded
                if (!context.MenuItems.Any())
                {
                    context.MenuItems.AddRange(
                        new MenuItem
                        {
                            Name = "Margherita Pizza",
                            Description = "Classic pizza with tomato sauce, mozzarella cheese, and fresh basil.",
                            Price = 12.99m,
                            Category = "Pizza",
                            ImageUrl = "/Img/menu/margherita_pizza.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Pepperoni Pizza",
                            Description = "Pizza topped with pepperoni, mozzarella cheese, and tomato sauce.",
                            Price = 14.99m,
                            Category = "Pizza",
                            ImageUrl = "/Img/menu/pepperoni_pizza.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "BBQ Chicken Pizza",
                            Description = "Pizza with BBQ sauce, chicken, red onions, and cilantro.",
                            Price = 15.99m,
                            Category = "Pizza",
                            ImageUrl = "/Img/menu/bbq_chicken_pizza.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Caesar Salad",
                            Description = "Crisp romaine lettuce, croutons, and Caesar dressing.",
                            Price = 8.99m,
                            Category = "Salad",
                            ImageUrl = "/Img/menu/caesar_salad.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Greek Salad",
                            Description = "Salad with cucumbers, tomatoes, olives, feta cheese, and Greek dressing.",
                            Price = 9.99m,
                            Category = "Salad",
                            ImageUrl = "/Img/menu/greek_salad.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "House Salad",
                            Description = "Mixed greens, tomatoes, cucumbers, red onions, and house dressing.",
                            Price = 7.99m,
                            Category = "Salad",
                            ImageUrl = "/Img/menu/house_salad.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Spaghetti Carbonara",
                            Description = "Spaghetti with pancetta, eggs, and Parmesan cheese.",
                            Price = 14.99m,
                            Category = "Pasta",
                            ImageUrl = "/Img/menu/spaghetti_carbonara.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Fettuccine Alfredo",
                            Description = "Fettuccine pasta with creamy Alfredo sauce.",
                            Price = 13.99m,
                            Category = "Pasta",
                            ImageUrl = "/Img/menu/fettuccine_alfredo.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
                        },
                        new MenuItem
                        {
                            Name = "Penne Arrabbiata",
                            Description = "Penne pasta in a spicy tomato sauce.",
                            Price = 12.99m,
                            Category = "Pasta",
                            ImageUrl = "/Img/menu/penne_arrabbiata.jpg",
                            IsAvailable = true,
                            CreatedById = adminUser.Id
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
                            Name = "Tomatoes",
                            Description = "Fresh organic tomatoes.",
                            Quantity = 100,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Mozzarella Cheese",
                            Description = "Creamy mozzarella cheese.",
                            Quantity = 50,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Pasta",
                            Description = "High-quality durum wheat pasta.",
                            Quantity = 200,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Olive Oil",
                            Description = "Extra virgin olive oil.",
                            Quantity = 30,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Basil",
                            Description = "Fresh basil leaves.",
                            Quantity = 50,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Chicken Breast",
                            Description = "Boneless, skinless chicken breast.",
                            Quantity = 25,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Red Onions",
                            Description = "Fresh red onions.",
                            Quantity = 40,
                            CreatedById = adminUser.Id
                        },
                        new InventoryItem
                        {
                            Name = "Parmesan Cheese",
                            Description = "Grated Parmesan cheese.",
                            Quantity = 60,
                            CreatedById = adminUser.Id
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
    }
}
