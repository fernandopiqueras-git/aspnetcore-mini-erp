using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MiniErp.Controllers;

public class CultureController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl)
    {
        var selectedCulture = culture == "en-US" ? "en-US" : "es-ES";
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selectedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }
}
