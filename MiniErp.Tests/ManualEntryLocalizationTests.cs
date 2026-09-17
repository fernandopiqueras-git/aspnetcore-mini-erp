using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MiniErp.Controllers;
using MiniErp.Data;
using MiniErp.ViewModels;

namespace MiniErp.Tests;

public class ManualEntryLocalizationTests
{
    [Theory]
    [InlineData("es-ES", "Entrada manual")]
    [InlineData("en-US", "Manual receipt")]
    public void ManualEntry_UsesTheSelectedLanguage(string cultureName, string expectedReference)
    {
        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            using var database = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

            var result = new StockMovementsController(database).ManualEntry();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StockMovementViewModel>(view.Model);
            Assert.Equal(expectedReference, model.Reference);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }
}
