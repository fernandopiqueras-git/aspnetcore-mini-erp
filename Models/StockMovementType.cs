using System.ComponentModel.DataAnnotations;

namespace MiniErp.Models;

public enum StockMovementType
{
    [Display(Name = "Entrada")]
    Entry,
    [Display(Name = "Salida")]
    Exit,
    [Display(Name = "Transferencia")]
    Transfer,
    [Display(Name = "Regularización")]
    Adjustment
}
