using System.ComponentModel.DataAnnotations;
namespace MiniErp.Models;
public class AuditEntry { public int Id { get; set; } [Required, StringLength(160)] public string UserName { get; set; } = string.Empty; [Required, StringLength(30)] public string Action { get; set; } = string.Empty; [Required, StringLength(100)] public string EntityName { get; set; } = string.Empty; [StringLength(100)] public string EntityId { get; set; } = string.Empty; public DateTime Timestamp { get; set; } }
