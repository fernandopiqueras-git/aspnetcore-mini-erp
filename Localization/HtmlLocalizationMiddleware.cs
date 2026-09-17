using System.Globalization;
using System.Text;

namespace MiniErp.Localization;

public class HtmlLocalizationMiddleware(RequestDelegate next)
{
    private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
    {
        ["No se puede eliminar un artículo con pedidos, existencias o movimientos de stock."] = "A product with orders, stock or inventory movements cannot be deleted.",
        ["El stock se modifica mediante entradas, salidas, transferencias y regularizaciones."] = "Stock is changed through receipts, issues, transfers and adjustments.",
        ["Stock actual"] = "Current stock",
        ["No se puede eliminar un cliente utilizado en pedidos."] = "A customer used in orders cannot be deleted.",
        ["No se puede eliminar un artículo utilizado en pedidos."] = "A product used in orders cannot be deleted.",
        ["Ya existe un artículo con este SKU."] = "A product with this SKU already exists.",
        ["El código ya existe."] = "The code already exists.",
        ["La operación se bloqueará si el cliente está utilizado en algún pedido."] = "The operation will be blocked if the customer is used in an order.",
        ["La operación se bloqueará si el artículo está utilizado en algún pedido."] = "The operation will be blocked if the product is used in an order.",
        ["¿Seguro que quieres eliminar"] = "Are you sure you want to delete",
        ["¿Quieres eliminar"] = "Do you want to delete",
        ["Confirmar eliminación"] = "Confirm deletion",
        ["Volver a clientes"] = "Back to customers",
        ["Volver a artículos"] = "Back to products",
        ["Detalle de cliente"] = "Customer details",
        ["Detalle de artículo"] = "Product details",
        ["Editar cliente"] = "Edit customer",
        ["Editar artículo"] = "Edit product",
        ["Editar proveedor"] = "Edit supplier",
        ["Editar almacén"] = "Edit warehouse",
        ["Editar pedido"] = "Edit order",
        ["Eliminar cliente"] = "Delete customer",
        ["Eliminar artículo"] = "Delete product",
        ["Eliminar proveedor"] = "Delete supplier",
        ["Nuevo almacén"] = "New warehouse",
        ["Nueva compra"] = "New purchase",
        ["Pedidos de compra"] = "Purchase orders",
        ["Todos los estados"] = "All statuses",
        ["Número o proveedor"] = "Number or supplier",
        ["Número o cliente"] = "Number or customer",
        ["Correo electrónico"] = "Email address",
        ["Cliente activo"] = "Active customer",
        ["Artículo activo"] = "Active product",
        ["Base de gestión comercial para clientes, artículos y pedidos."] = "Sales management foundation for customers, products and orders.",
        ["Clientes activos"] = "Active customers",
        ["Artículos activos"] = "Active products",
        ["Pedidos abiertos"] = "Open orders",
        ["Últimos pedidos"] = "Latest orders",
        ["MVP inicial"] = "Initial MVP",
        ["registros"] = "records",
        ["Líneas de pedido"] = "Order lines",
        ["Añadir línea"] = "Add line",
        ["Quitar"] = "Remove",
        ["Líneas"] = "Lines",
        ["Pedidos"] = "Orders",
        ["Pedido"] = "Order",
        ["Cobros/Pagos"] = "Receipts/Payments",
        ["Impuestos"] = "Taxes",
        ["Método"] = "Method",
        ["Importe"] = "Amount",
        ["Pendiente"] = "Outstanding",
        ["Vencimiento"] = "Due date",
        ["Emitir"] = "Issue",
        ["Recibir"] = "Receive",
        ["Completar"] = "Complete",
        ["Confirmar"] = "Confirm",
        ["Desactivar"] = "Deactivate",
        ["Existencias"] = "Stock",
        ["Referencias"] = "References",
        ["Dto."] = "Disc.",
        ["No se puede eliminar el proveedor porque tiene pedidos de compra. Puedes desactivarlo desde Editar."] = "The supplier cannot be deleted because it has purchase orders. You can deactivate it from Edit.",
        ["No se puede eliminar el cliente porque está utilizado en pedidos."] = "The customer cannot be deleted because it is used in orders.",
        ["No se puede eliminar el artículo porque está utilizado en pedidos."] = "The product cannot be deleted because it is used in orders.",
        ["Todas las líneas deben usar artículos activos."] = "All lines must use active products.",
        ["Ya existe un pedido con este número."] = "An order with this number already exists.",
        ["Ya existe un proveedor con este NIF o CIF."] = "A supplier with this tax ID already exists.",
        ["Ya existe un cliente con este NIF o CIF."] = "A customer with this tax ID already exists.",
        ["Selecciona un almacén de destino."] = "Select the destination warehouse.",
        ["Selecciona el almacén de origen."] = "Select the source warehouse.",
        ["Selecciona un almacén activo."] = "Select an active warehouse.",
        ["Selecciona un cliente activo."] = "Select an active customer.",
        ["Los almacenes deben ser distintos."] = "The warehouses must be different.",
        ["Pedido de compra"] = "Purchase order",
        ["Pedido de venta"] = "Sales order",
        ["Movimientos de stock"] = "Stock movements",
        ["Entrada manual de stock"] = "Manual stock receipt",
        ["Registrar entrada"] = "Record receipt",
        ["Nuevo movimiento"] = "New movement",
        ["Entrada manual"] = "Manual receipt",
        ["Regularización manual"] = "Manual adjustment",
        ["Destino no disponible"] = "Destination unavailable",
        ["Origen no disponible"] = "Origin unavailable",
        ["Todos los artículos"] = "All products",
        ["Todos los almacenes"] = "All warehouses",
        ["Nuevo proveedor"] = "New supplier",
        ["Nuevo artículo"] = "New product",
        ["Nuevo cliente"] = "New customer",
        ["Nuevo pedido"] = "New order",
        ["Nueva factura"] = "New invoice",
        ["Crear factura"] = "Create invoice",
        ["Cancelar factura"] = "Cancel invoice",
        ["Buscar por nombre o NIF/CIF"] = "Search by name or tax ID",
        ["Buscar por nombre o SKU"] = "Search by name or SKU",
        ["Nombre o NIF/CIF"] = "Name or tax ID",
        ["Motivo o documento de referencia"] = "Reason or reference document",
        ["Selecciona un artículo"] = "Select a product",
        ["Selecciona un almacén"] = "Select a warehouse",
        ["Selecciona un cliente"] = "Select a customer",
        ["Selecciona un proveedor"] = "Select a supplier",
        ["Cantidad o nuevo stock"] = "Quantity or new stock",
        ["Almacén destino"] = "Destination warehouse",
        ["Almacén origen"] = "Source warehouse",
        ["Stock insuficiente."] = "Insufficient stock.",
        ["Artículo no válido."] = "Invalid product.",
        ["Proveedor no válido."] = "Invalid supplier.",
        ["Cliente no válido."] = "Invalid customer.",
        ["Almacén no válido."] = "Invalid warehouse.",
        ["Número duplicado."] = "Duplicate number.",
        ["Confirmar pedido"] = "Confirm order",
        ["Completar pedido"] = "Complete order",
        ["Cancelar pedido"] = "Cancel order",
        ["Recibir pedido"] = "Receive order",
        ["Emitir factura"] = "Issue invoice",
        ["Registrar pago"] = "Record payment",
        ["Pedidos recientes"] = "Recent orders",
        ["Ventas pendientes"] = "Pending sales",
        ["Compras pendientes"] = "Pending purchases",
        ["Facturas pendientes"] = "Pending invoices",
        ["Total facturado"] = "Total invoiced",
        ["Panel de gestión"] = "Management dashboard",
        ["Gestión comercial"] = "Sales management",
        ["Gestión de compras"] = "Purchasing",
        ["Datos generales"] = "General information",
        ["Líneas del pedido"] = "Order lines",
        ["Dirección"] = "Address",
        ["Teléfono"] = "Phone",
        ["Correo"] = "Email",
        ["Precio unitario"] = "Unit price",
        ["Precio"] = "Price",
        ["Descuento"] = "Discount",
        ["Subtotal"] = "Subtotal",
        ["Cantidad"] = "Quantity",
        ["Referencia"] = "Reference",
        ["Artículo"] = "Product",
        ["Artículos"] = "Products",
        ["Clientes"] = "Customers",
        ["Cliente"] = "Customer",
        ["Proveedores"] = "Suppliers",
        ["Proveedor"] = "Supplier",
        ["Almacenes"] = "Warehouses",
        ["Almacén"] = "Warehouse",
        ["Movimientos"] = "Movements",
        ["Facturación"] = "Invoicing",
        ["Facturas"] = "Invoices",
        ["Factura"] = "Invoice",
        ["Compras"] = "Purchases",
        ["Compra"] = "Purchase",
        ["Ventas"] = "Sales",
        ["Venta"] = "Sale",
        ["Panel"] = "Dashboard",
        ["Fecha de vencimiento"] = "Due date",
        ["Fecha de pedido"] = "Order date",
        ["Fecha"] = "Date",
        ["Número"] = "Number",
        ["Estado"] = "Status",
        ["Origen"] = "Source",
        ["Destino"] = "Destination",
        ["NIF/CIF"] = "Tax ID",
        ["Activo"] = "Active",
        ["Activos"] = "Active",
        ["Inactivo"] = "Inactive",
        ["Inactivos"] = "Inactive",
        ["Borrador"] = "Draft",
        ["Confirmado"] = "Confirmed",
        ["Completado"] = "Completed",
        ["Recibido"] = "Received",
        ["Emitida"] = "Issued",
        ["Pagada"] = "Paid",
        ["Cancelado"] = "Cancelled",
        ["Entrada"] = "Receipt",
        ["Salida"] = "Issue",
        ["Transferencia"] = "Transfer",
        ["Regularización"] = "Adjustment",
        ["Draft"] = "Draft",
        ["Confirmed"] = "Confirmed",
        ["Completed"] = "Completed",
        ["Received"] = "Received",
        ["Issued"] = "Issued",
        ["Paid"] = "Paid",
        ["Cancelled"] = "Cancelled",
        ["Entry"] = "Receipt",
        ["Exit"] = "Issue",
        ["Adjustment"] = "Adjustment",
        ["Sale"] = "Sale",
        ["Purchase"] = "Purchase",
        ["Crear"] = "Create",
        ["Editar"] = "Edit",
        ["Eliminar"] = "Delete",
        ["Detalles"] = "Details",
        ["Ver"] = "View",
        ["Guardar"] = "Save",
        ["Cancelar"] = "Cancel",
        ["Volver"] = "Back",
        ["Limpiar"] = "Clear",
        ["Filtrar"] = "Filter",
        ["Buscar"] = "Search",
        ["Todos"] = "All",
        ["Todas"] = "All",
        ["Sí"] = "Yes",
        ["No"] = "No",
        ["Total"] = "Total",
        ["Nombre"] = "Name",
        ["Código"] = "Code",
        ["Descripción"] = "Description",
        ["Idioma"] = "Language",
        ["MAESTROS"] = "MASTER DATA",
        ["ALMACÉN"] = "WAREHOUSE"
    };

    public async Task Invoke(HttpContext context)
    {
        if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "en")
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
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
            var html = TranslateMarkup(await reader.ReadToEndAsync());
            var output = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength = output.Length;
            await originalBody.WriteAsync(output);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static string TranslateMarkup(string html)
    {
        var result = new StringBuilder(html.Length);
        var position = 0;

        while (position < html.Length)
        {
            var tagStart = html.IndexOf('<', position);
            if (tagStart < 0)
            {
                result.Append(TranslateText(html[position..]));
                break;
            }

            result.Append(TranslateText(html[position..tagStart]));
            var tagEnd = html.IndexOf('>', tagStart);
            if (tagEnd < 0)
            {
                result.Append(html[tagStart..]);
                break;
            }

            var tag = html[tagStart..(tagEnd + 1)];
            result.Append(TranslateAttributes(tag));

            var rawElement = tag.StartsWith("<script", StringComparison.OrdinalIgnoreCase)
                ? "script"
                : tag.StartsWith("<style", StringComparison.OrdinalIgnoreCase) ? "style" : null;
            if (rawElement is not null)
            {
                var closingStart = html.IndexOf($"</{rawElement}", tagEnd + 1, StringComparison.OrdinalIgnoreCase);
                var closingEnd = closingStart < 0 ? -1 : html.IndexOf('>', closingStart);
                if (closingEnd < 0)
                {
                    result.Append(html[(tagEnd + 1)..]);
                    break;
                }

                result.Append(html[(tagEnd + 1)..(closingEnd + 1)]);
                position = closingEnd + 1;
                continue;
            }

            position = tagEnd + 1;
        }

        return result.ToString();
    }

    private static string TranslateAttributes(string tag)
    {
        foreach (var attribute in new[] { "placeholder", "title", "aria-label" })
        {
            var marker = attribute + "=\"";
            var searchFrom = 0;
            while (true)
            {
                var valueStart = tag.IndexOf(marker, searchFrom, StringComparison.OrdinalIgnoreCase);
                if (valueStart < 0)
                    break;

                valueStart += marker.Length;
                var valueEnd = tag.IndexOf('"', valueStart);
                if (valueEnd < 0)
                    break;

                var translated = TranslateText(tag[valueStart..valueEnd]);
                tag = tag[..valueStart] + translated + tag[valueEnd..];
                searchFrom = valueStart + translated.Length + 1;
            }
        }

        return tag;
    }

    private static string TranslateText(string text)
    {
        foreach (var translation in English.OrderByDescending(item => item.Key.Length))
            text = text.Replace(translation.Key, translation.Value, StringComparison.Ordinal);
        return text;
    }
}
