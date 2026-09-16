using System.ComponentModel.DataAnnotations;
namespace MiniErp.Models;
public class Warehouse
{
 public int Id { get; set; }
 [Required,StringLength(20)] public string Code { get; set; }=string.Empty;
 [Required,StringLength(120)] public string Name { get; set; }=string.Empty;
 public bool IsActive { get; set; }=true;
 public ICollection<WarehouseStock> Stocks { get; set; }=[];
 public ICollection<StockMovement> SourceMovements { get; set; }=[];
 public ICollection<StockMovement> DestinationMovements { get; set; }=[];
}