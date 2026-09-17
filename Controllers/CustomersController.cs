using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Models;

namespace MiniErp.Controllers;

public class CustomersController(AppDbContext database) : Controller
{
    [HttpGet]
    public IActionResult Index(string? search, bool? active)
    {
        var query = database.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(customer => customer.Name.Contains(term) || customer.TaxId.Contains(term));
        }
        if (active.HasValue)
            query = query.Where(customer => customer.IsActive == active);

        ViewBag.Search = search;
        ViewBag.Active = active;
        return View(query.OrderBy(customer => customer.Name).ToArray());
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var customer = database.Customers.AsNoTracking().Include(item => item.SalesOrders).FirstOrDefault(item => item.Id == id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new Customer());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Customer customer)
    {
        Normalize(customer);
        ValidateDuplicateTaxId(customer);
        if (!ModelState.IsValid)
            return View("Form", customer);

        database.Customers.Add(customer);
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id = customer.Id });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var customer = database.Customers.AsNoTracking().FirstOrDefault(item => item.Id == id);
        return customer is null ? NotFound() : View("Form", customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Customer customer)
    {
        if (id != customer.Id)
            return BadRequest();
        var stored = database.Customers.FirstOrDefault(item => item.Id == id);
        if (stored is null)
            return NotFound();

        Normalize(customer);
        ValidateDuplicateTaxId(customer);
        if (!ModelState.IsValid)
            return View("Form", customer);

        stored.Name = customer.Name;
        stored.TaxId = customer.TaxId;
        stored.Email = customer.Email;
        stored.Phone = customer.Phone;
        stored.Address = customer.Address;
        stored.IsActive = customer.IsActive;
        database.SaveChanges();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var customer = database.Customers.AsNoTracking().FirstOrDefault(item => item.Id == id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var customer = database.Customers.Find(id);
        if (customer is null)
            return NotFound();
        if (database.SalesOrders.Any(order => order.CustomerId == id))
        {
            TempData["Error"] = "No se puede eliminar un cliente utilizado en pedidos.";
            return RedirectToAction(nameof(Details), new { id });
        }

        database.Customers.Remove(customer);
        database.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateDuplicateTaxId(Customer customer)
    {
        if (database.Customers.Any(item => item.Id != customer.Id && item.TaxId == customer.TaxId))
            ModelState.AddModelError(nameof(Customer.TaxId), "Ya existe un cliente con este NIF o CIF.");
    }

    private static void Normalize(Customer customer)
    {
        customer.Name = customer.Name?.Trim() ?? string.Empty;
        customer.TaxId = string.Concat((customer.TaxId ?? string.Empty).Where(char.IsLetterOrDigit)).ToUpperInvariant();
        customer.Email = EmptyToNull(customer.Email)?.ToLowerInvariant();
        customer.Phone = EmptyToNull(customer.Phone);
        customer.Address = EmptyToNull(customer.Address);
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
