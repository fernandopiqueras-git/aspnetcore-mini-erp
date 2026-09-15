using System.ComponentModel.DataAnnotations;

namespace MiniErp.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El NIF o CIF es obligatorio.")]
    [StringLength(20)]
    [SpanishTaxId]
    public string TaxId { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(160)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "El teléfono no es válido.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(240)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<SalesOrder> SalesOrders { get; set; } = [];
}
