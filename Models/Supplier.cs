using System.ComponentModel.DataAnnotations;
namespace MiniErp.Models;
public class Supplier
{
 public int Id { get; set; }
 [Required, StringLength(120)] public string Name { get; set; } = string.Empty;
 [Required, StringLength(20), SpanishTaxId] public string TaxId { get; set; } = string.Empty;
 [EmailAddress, StringLength(160)] public string? Email { get; set; }
 [Phone, StringLength(30)] public string? Phone { get; set; }
 [StringLength(240)] public string? Address { get; set; }
 public bool IsActive { get; set; } = true;
 public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
}