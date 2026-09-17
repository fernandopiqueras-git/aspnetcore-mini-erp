using System.Globalization;
using Microsoft.AspNetCore.Http;
using MiniErp.Localization;

namespace MiniErp.Tests;

public class LocalizationMiddlewareTests
{
    [Fact]
    public async Task EnglishTranslation_DoesNotModifyInputValuesOrRoutes()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new HtmlLocalizationMiddleware(async httpContext =>
            {
                httpContext.Response.ContentType = "text/html; charset=utf-8";
                await httpContext.Response.WriteAsync("<a href=\"/SalesOrders/Edit/1\">Ventas</a><input name=\"Name\" value=\"Ventas\" placeholder=\"Buscar\"><h1>Clientes</h1>");
            });

            await middleware.Invoke(context);

            context.Response.Body.Position = 0;
            var html = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.Contains("href=\"/SalesOrders/Edit/1\"", html);
            Assert.Contains("value=\"Ventas\"", html);
            Assert.Contains("placeholder=\"Search\"", html);
            Assert.Contains(">Sales</a>", html);
            Assert.Contains("<h1>Customers</h1>", html);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public async Task SpanishRequests_PassThroughUnchanged()
    {
        var previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es-ES");
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var middleware = new HtmlLocalizationMiddleware(httpContext => httpContext.Response.WriteAsync("<h1>Clientes</h1>"));

            await middleware.Invoke(context);

            context.Response.Body.Position = 0;
            var html = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.Equal("<h1>Clientes</h1>", html);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
}
