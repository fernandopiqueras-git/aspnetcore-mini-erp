using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MiniErp.Models;
public class PurchaseOrder
{
 public int Id { get; set; }
 [Required, StringLength(30)] public string Number { get; set; } = string.Empty;
 public DateTime OrderDate { get; set; } = DateTime.Today;
 public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
 [Range(1, int.MaxValue)] public int SupplierId { get; set; }
 public Supplier Supplier { get; set; } = null!;
 public ICollection<PurchaseOrderLine> Lines { get; set; } = [];
 [NotMapped] public decimal Total => Lines.Sum(x => x.LineTotal);
}