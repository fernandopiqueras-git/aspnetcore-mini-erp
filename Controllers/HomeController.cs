using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;
using MiniErp.ViewModels;

namespace MiniErp.Controllers;

public class HomeController(AppDbContext database) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var orders = database.SalesOrders
            .AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Lines)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .ToArray();
        var model = new DashboardViewModel
        {
            ActiveCustomers = database.Customers.Count(customer => customer.IsActive),
            ActiveProducts = database.Products.Count(product => product.IsActive),
            OpenOrders = orders.Count(order => order.Status is SalesOrderStatus.Draft or SalesOrderStatus.Confirmed),
            SalesTotal = orders.Where(order => order.Status == SalesOrderStatus.Completed).Sum(order => order.Total),
            RecentOrders = orders.Take(5).ToArray()
        };

        return View(model);
    }
}
