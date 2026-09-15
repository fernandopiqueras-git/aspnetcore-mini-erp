using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MiniErp.Models;
public class PurchaseOrderLine
{
 public int Id { get; set; }
 public int PurchaseOrderId { get; set; }
 public PurchaseOrder PurchaseOrder { get; set; } = null!;
 [Range(1, int.MaxValue)] public int ProductId { get; set; }
 public Product Product { get; set; } = null!;
 [Range(typeof(decimal), "0.001", "999999999.999", ParseLimitsInInvariantCulture = true)] public decimal Quantity { get; set; }
 [Range(typeof(decimal), "0", "999999999.99", ParseLimitsInInvariantCulture = true)] public decimal UnitPrice { get; set; }
 [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true)] public decimal DiscountPercentage { get; set; }
 [NotMapped] public decimal LineTotal => decimal.Round(Quantity * UnitPrice * (1 - DiscountPercentage / 100), 2);
}