using System.Globalization;
using System.Text;

namespace MiniErp.Localization;

public class SpanishEnumLocalizationMiddleware(RequestDelegate next)
{
    private static readonly IReadOnlyDictionary<string, string> Translations = new Dictionary<string, string>
    {
        [">Confirmed<"] = ">Confirmado<",
        [">Completed<"] = ">Completado<",
        [">Cancelled<"] = ">Cancelado<",
        [">Received<"] = ">Recibido<",
        [">Adjustment<"] = ">Regularización<",
        [">Transfer<"] = ">Transferencia<",
        [">Purchase<"] = ">Compra<",
        [">Draft<"] = ">Borrador<",
        [">Issued<"] = ">Emitida<",
        [">Paid<"] = ">Pagada<",
        [">Entry<"] = ">Entrada<",
        [">Exit<"] = ">Salida<",
        [">Sale<"] = ">Venta<"
    };

    public async Task Invoke(HttpContext context)
    {
        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "es")
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next(context);

        context.Response.Body = originalBody;
        if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) != true)
        {
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            return;
        }

        buffer.Position = 0;
        using var reader = new StreamReader(buffer, Encoding.UTF8);
        var html = await reader.ReadToEndAsync();

        foreach (var translation in Translations)
            html = html.Replace(translation.Key, translation.Value, StringComparison.Ordinal);

        var output = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength = output.Length;
        await originalBody.WriteAsync(output);
    }
}
