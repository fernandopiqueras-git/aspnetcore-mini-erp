using System.ComponentModel.DataAnnotations;

namespace MiniErp.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(40)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999.99")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999.999")]
    public decimal Stock { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<SalesOrderLine> SalesOrderLines { get; set; } = [];
}
