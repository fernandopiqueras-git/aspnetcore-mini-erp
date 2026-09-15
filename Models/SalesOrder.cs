using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniErp.Models;

public class SalesOrder
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Number { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.Today;
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<SalesOrderLine> Lines { get; set; } = [];

    [NotMapped]
    public decimal Total => Lines.Sum(line => line.LineTotal);
}
