using Microsoft.AspNetCore.Identity;
using MiniErp.Models;
using MiniErp.Security;

namespace MiniErp.Data;

public static class IdentityData
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var email = configuration["AdminUser:Email"] ?? "admin@minierp.local";
        var password = configuration["AdminUser:Password"] ?? "ChangeMe123!";
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var administrator = await userManager.FindByEmailAsync(email);
        if (administrator is null)
        {
            administrator = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(administrator, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
        if (!await userManager.IsInRoleAsync(administrator, AppRoles.Administrator))
            await userManager.AddToRoleAsync(administrator, AppRoles.Administrator);
    }
}
