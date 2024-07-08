using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using DiningHub.Helper;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireInternalRole")]
    [Route("report")]
    public class ReportController : Controller
    {
        private readonly DiningHubContext _context;

        public ReportController(DiningHubContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTimeHelper.GetMalaysiaTime().AddMonths(-1);
            endDate ??= DateTimeHelper.GetMalaysiaTime();

            var stats = new
            {
                TotalOrders = await _context.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalRevenue = await _context.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).SumAsync(o => o.TotalAmount),
                TopSellingItems = await GetTopSellingItemsAsync((DateTime)startDate, (DateTime)endDate)
            };

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Stats = stats;

            return View();
        }

        [HttpGet("order")]
        public async Task<IActionResult> OrderReport(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTimeHelper.GetMalaysiaTime().AddMonths(-1);
            endDate ??= DateTimeHelper.GetMalaysiaTime();

            var popularProducts = await GetPopularProductsAsync((DateTime)startDate, (DateTime)endDate);
            var popularCategories = await GetPopularCategoriesAsync((DateTime)startDate, (DateTime)endDate);
            var monthlySales = await GetMonthlySalesAsync((DateTime)startDate, (DateTime)endDate);
            var categoryOrderQuantities = await GetCategoryOrderQuantitiesAsync((DateTime)startDate, (DateTime)endDate);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.PopularProducts = popularProducts;
            ViewBag.PopularCategories = popularCategories;
            ViewBag.MonthlySales = monthlySales;
            ViewBag.CategoryOrderQuantities = categoryOrderQuantities;

            return View();
        }

        [HttpGet("user")]
        public async Task<IActionResult> UserReport(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTimeHelper.GetMalaysiaTime().AddMonths(-1);
            endDate ??= DateTimeHelper.GetMalaysiaTime();

            var userActivity = await GetUserActivityAsync((DateTime)startDate, (DateTime)endDate);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.UserActivity = userActivity;

            return View();
        }

        private async Task<IEnumerable<dynamic>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate)
        {
            var query = _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate);

            return await query
                .GroupBy(oi => new { oi.MenuItem.Name })
                .Select(g => new { g.Key.Name, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetPopularProductsAsync(DateTime startDate, DateTime endDate)
        {
            var query = _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate);

            return await query
                .GroupBy(oi => new { oi.MenuItem.Name })
                .Select(g => new { g.Key.Name, Count = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetPopularCategoriesAsync(DateTime startDate, DateTime endDate)
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.MenuItem)
                .ThenInclude(mi => mi.Category)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate && !oi.MenuItem.IsDeleted)
                .ToListAsync();

            var groupedResult = orderItems
                .GroupBy(oi => oi.MenuItem.Category)
                .Select(g => new
                {
                    Category = g.Key.Name,
                    Count = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return groupedResult;
        }

        private async Task<IEnumerable<dynamic>> GetMonthlySalesAsync(DateTime startDate, DateTime endDate)
        {
            var query = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            return await query
                .GroupBy(o => new { Month = o.OrderDate.Month })
                .Select(g => new { Month = g.Key.Month, TotalSales = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Month)
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetCategoryOrderQuantitiesAsync(DateTime startDate, DateTime endDate)
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.MenuItem)
                .ThenInclude(mi => mi.Category)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate && !oi.MenuItem.IsDeleted)
                .ToListAsync();

            var groupedResult = orderItems
                .GroupBy(oi => oi.MenuItem.Category)
                .Select(g => new
                {
                    Category = g.Key.Name,
                    Quantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .ToList();

            return groupedResult;
        }


        private async Task<IEnumerable<dynamic>> GetUserActivityAsync(DateTime startDate, DateTime endDate)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            return await query
                .GroupBy(o => new { o.User.UserName })
                .Select(g => new
                {
                    g.Key.UserName,
                    TotalOrders = g.Count(),
                    TotalSpend = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.TotalOrders)
                .ToListAsync();
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> InventoryReport()
        {
            var inventoryItems = await GetInventoryItemsAsync();
            var lowStockItems = await GetLowStockItemsAsync();

            ViewBag.InventoryItems = inventoryItems;
            ViewBag.LowStockItems = lowStockItems;

            return View();
        }

        private async Task<IEnumerable<dynamic>> GetInventoryItemsAsync()
        {
            return await _context.InventoryItems
                .Select(ii => new { ii.Name, ii.Quantity })
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetLowStockItemsAsync()
        {
            return await _context.InventoryItems
                .Where(ii => ii.Quantity < 10)
                .Select(ii => new { ii.Name, ii.Quantity })
                .ToListAsync();
        }
    }
}
