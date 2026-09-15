using System.ComponentModel.DataAnnotations;
using MiniErp.Models;
namespace MiniErp.ViewModels;
public class PurchaseOrderFormViewModel : IValidatableObject
{
 public int Id { get; set; }
 [Required, StringLength(30)] public string Number { get; set; } = string.Empty;
 [DataType(DataType.Date)] public DateTime OrderDate { get; set; } = DateTime.Today;
 [Range(1, int.MaxValue)] public int SupplierId { get; set; }
 public PurchaseOrderStatus Status { get; set; }
 public List<PurchaseOrderLineInput> Lines { get; set; } = [new()];
 public IEnumerable<ValidationResult> Validate(ValidationContext context)
 {
  if (Lines.Count == 0) yield return new("El pedido debe tener líneas.", [nameof(Lines)]);
  if (Lines.Where(x => x.ProductId > 0).GroupBy(x => x.ProductId).Any(x => x.Count() > 1)) yield return new("No se puede repetir un artículo.", [nameof(Lines)]);
  if (Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled) yield return new("El estado final se gestiona desde las acciones.", [nameof(Status)]);
 }
}
public class PurchaseOrderLineInput
{
 [Range(1, int.MaxValue)] public int ProductId { get; set; }
 [Range(typeof(decimal), "0.001", "999999999.999", ParseLimitsInInvariantCulture = true)] public decimal Quantity { get; set; } = 1;
 [Range(typeof(decimal), "0", "999999999.99", ParseLimitsInInvariantCulture = true)] public decimal UnitPrice { get; set; }
 [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true)] public decimal DiscountPercentage { get; set; }
}