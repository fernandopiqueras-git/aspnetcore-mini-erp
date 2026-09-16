using MiniErp.Models;

namespace MiniErp.Data;

public static class DemoData
{
    public static void Seed(AppDbContext database)
    {
        var generalSupplier = database.Suppliers.FirstOrDefault(supplier => supplier.TaxId == "B12345674");
        var generalSupplierCreated = generalSupplier is null;

        if (generalSupplierCreated)
        {
            generalSupplier = new Supplier
            {
                Name = "Proveedor general",
                TaxId = "B12345674",
                Email = "compras@proveedorgeneral.example",
                Phone = "900000000",
                Address = "España"
            };

            database.Suppliers.Add(generalSupplier);
            database.SaveChanges();

            var purchaseOrder = database.PurchaseOrders.FirstOrDefault(order => order.Number == "PV-2026-0002");
            if (purchaseOrder is not null)
            {
                purchaseOrder.SupplierId = generalSupplier.Id;
                database.SaveChanges();
            }
        }

        if (database.Customers.Any() || database.Products.Any() || database.SalesOrders.Any())
            return;

        var customer = new Customer
        {
            Name = "Distribuciones La Mancha",
            TaxId = "B45000007",
            Email = "compras@lamancha.example",
            Phone = "925000001",
            Address = "Toledo"
        };
        var products = new[]
        {
            new Product { Sku = "ART-001", Name = "Artículo estándar", UnitPrice = 24.90m, Stock = 120m },
            new Product { Sku = "ART-002", Name = "Artículo premium", UnitPrice = 49.50m, Stock = 65m }
        };
        var order = new SalesOrder
        {
            Number = "PV-2026-0001",
            Customer = customer,
            OrderDate = DateTime.Today,
            Status = SalesOrderStatus.Confirmed,
            WarehouseId = 1,
            Lines =
            [
                new SalesOrderLine { Product = products[0], Quantity = 5m, UnitPrice = products[0].UnitPrice },
                new SalesOrderLine { Product = products[1], Quantity = 2m, UnitPrice = products[1].UnitPrice, DiscountPercentage = 5m }
            ]
        };

        database.SalesOrders.Add(order);
        database.WarehouseStocks.AddRange(
            new WarehouseStock { WarehouseId = 1, Product = products[0], Quantity = products[0].Stock },
            new WarehouseStock { WarehouseId = 1, Product = products[1], Quantity = products[1].Stock });
        database.SaveChanges();
    }
}
