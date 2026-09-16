using System.ComponentModel.DataAnnotations;
using MiniErp.Models;

namespace MiniErp.ViewModels;

public class SalesOrderFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El número es obligatorio.")]
    [StringLength(30)]
    public string Number { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente.")]
    public int CustomerId { get; set; }

    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un almacén.")] public int WarehouseId { get; set; }
    public List<SalesOrderLineInput> Lines { get; set; } = [new()];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Lines.Count == 0)
            yield return new ValidationResult("El pedido debe tener al menos una línea.", [nameof(Lines)]);

        var repeated = Lines.Where(line => line.ProductId > 0)
            .GroupBy(line => line.ProductId)
            .Any(group => group.Count() > 1);
        if (repeated)
            yield return new ValidationResult("No se puede repetir un artículo.", [nameof(Lines)]);

        if (Status is SalesOrderStatus.Completed or SalesOrderStatus.Cancelled)
            yield return new ValidationResult("El estado final se gestiona desde las acciones del pedido.", [nameof(Status)]);
    }
}

public class SalesOrderLineInput
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un artículo.")]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999.999", ParseLimitsInInvariantCulture = true, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public decimal Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "El precio no puede ser negativo.")]
    public decimal UnitPrice { get; set; }

    [Range(typeof(decimal), "0", "100", ParseLimitsInInvariantCulture = true, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
    public decimal DiscountPercentage { get; set; }
}