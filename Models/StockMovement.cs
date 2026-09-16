using System.ComponentModel.DataAnnotations;
namespace MiniErp.Models;
public class StockMovement
{
 public long Id { get; set; }
 public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
 public StockMovementType Type { get; set; }
 public int ProductId { get; set; }
 public Product Product { get; set; }=null!;
 public int? SourceWarehouseId { get; set; }
 public Warehouse? SourceWarehouse { get; set; }
 public int? DestinationWarehouseId { get; set; }
 public Warehouse? DestinationWarehouse { get; set; }
 [Range(typeof(decimal),"0.001","999999999.999",ParseLimitsInInvariantCulture=true)] public decimal Quantity { get; set; }
 [Required,StringLength(160)] public string Reference { get; set; }=string.Empty;
}