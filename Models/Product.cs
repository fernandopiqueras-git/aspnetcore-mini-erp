using System.ComponentModel.DataAnnotations;

namespace MiniErp.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(40)]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999.999", ParseLimitsInInvariantCulture = true, ErrorMessage = "El stock no puede ser negativo.")]
    public decimal Stock { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<SalesOrderLine> SalesOrderLines { get; set; } = [];
    public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = [];
}
