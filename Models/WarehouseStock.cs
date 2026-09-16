using System.ComponentModel.DataAnnotations;
namespace MiniErp.Models;
public class WarehouseStock
{
 public int WarehouseId { get; set; }
 public Warehouse Warehouse { get; set; }=null!;
 public int ProductId { get; set; }
 public Product Product { get; set; }=null!;
 [Range(typeof(decimal),"0","999999999.999",ParseLimitsInInvariantCulture=true)] public decimal Quantity { get; set; }
 [Timestamp] public byte[] RowVersion { get; set; }=[];
}