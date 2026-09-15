using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MiniErp.Models;

public partial class SpanishTaxIdAttribute : ValidationAttribute
{
    private const string ControlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    public SpanishTaxIdAttribute() : base("El NIF o CIF no es válido.") { }

    public override bool IsValid(object? value)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return true;

        var taxId = NonAlphanumeric().Replace(text.ToUpperInvariant(), string.Empty);
        if (Regex.IsMatch(taxId, @"^\d{8}[A-Z]$"))
            return taxId[8] == ControlLetters[int.Parse(taxId[..8]) % 23];
        if (Regex.IsMatch(taxId, @"^[XYZ]\d{7}[A-Z]$"))
        {
            var prefix = taxId[0] switch { 'X' => "0", 'Y' => "1", _ => "2" };
            return taxId[8] == ControlLetters[int.Parse(prefix + taxId[1..8]) % 23];
        }
        if (!Regex.IsMatch(taxId, @"^[ABCDEFGHJNPQRSUVW]\d{7}[0-9A-J]$"))
            return false;

        var sum = 0;
        for (var index = 1; index <= 7; index++)
        {
            var digit = taxId[index] - '0';
            if (index % 2 == 0)
                sum += digit;
            else
            {
                var doubled = digit * 2;
                sum += doubled / 10 + doubled % 10;
            }
        }
        var control = (10 - sum % 10) % 10;
        var expected = "JABCDEFGHI"[control];
        return taxId[8] == (char)('0' + control) || taxId[8] == expected;
    }

    [GeneratedRegex("[^A-Z0-9]")]
    private static partial Regex NonAlphanumeric();
}
