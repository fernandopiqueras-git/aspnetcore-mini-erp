using System.ComponentModel.DataAnnotations;
using MiniErp.Models;
namespace MiniErp.ViewModels;
public class StockMovementViewModel : IValidatableObject
{
 [Range(1,int.MaxValue)] public int ProductId { get; set; }
 public StockMovementType Type { get; set; }
 public int? SourceWarehouseId { get; set; }
 public int? DestinationWarehouseId { get; set; }
 [Range(typeof(decimal),"0.001","999999999.999",ParseLimitsInInvariantCulture=true)] public decimal Quantity { get; set; }
 [Required,StringLength(160)] public string Reference { get; set; }=string.Empty;
 public IEnumerable<ValidationResult> Validate(ValidationContext context)
 {
  if(Type is StockMovementType.Exit or StockMovementType.Transfer && !SourceWarehouseId.HasValue)yield return new("Selecciona el almacén de origen.",[nameof(SourceWarehouseId)]);
  if(Type is StockMovementType.Entry or StockMovementType.Transfer or StockMovementType.Adjustment && !DestinationWarehouseId.HasValue)yield return new("Selecciona el almacén de destino.",[nameof(DestinationWarehouseId)]);
  if(Type==StockMovementType.Transfer&&SourceWarehouseId==DestinationWarehouseId)yield return new("Los almacenes deben ser distintos.",[nameof(DestinationWarehouseId)]);
 }
}