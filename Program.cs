using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Localization;
using MiniErp.Options;
using MiniErp.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.Configure<CompanyOptions>(builder.Configuration.GetSection("Company"));
builder.Services.AddSingleton<InvoicePdfService>();
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    database.Database.Migrate();
    DemoData.Seed(database);
}

var supportedCultures = new[]
{
    new CultureInfo("es-ES"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<SpanishEnumLocalizationMiddleware>();
app.UseMiddleware<HtmlLocalizationMiddleware>();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

public partial class Program;
