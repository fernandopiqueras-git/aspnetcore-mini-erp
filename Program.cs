using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MiniErp.Data;
using MiniErp.Localization;
using MiniErp.Models;
using MiniErp.Options;
using MiniErp.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<CompanyOptions>(builder.Configuration.GetSection("Company"));
builder.Services.AddSingleton<InvoicePdfService>();
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var database = services.GetRequiredService<AppDbContext>();
    database.Database.Migrate();
    DemoData.Seed(database);
    if (args.Contains("--seed-admin", StringComparer.OrdinalIgnoreCase))
        await IdentityData.SeedAsync(services, builder.Configuration);
}
var supportedCultures = new[] { new CultureInfo("es-ES"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture("es-ES"), SupportedCultures = supportedCultures, SupportedUICultures = supportedCultures });
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<SpanishEnumLocalizationMiddleware>();
app.UseMiddleware<HtmlLocalizationMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

public partial class Program;
