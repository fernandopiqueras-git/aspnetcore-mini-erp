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
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureSucceeded(roleResult);
        }

        var email = configuration["AdminUser:Email"] ?? "admin@minierp.local";
        var password = configuration["AdminUser:Password"] ?? "ChangeMe123!";
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var administrator = await userManager.FindByEmailAsync(email);

        if (administrator is null)
        {
            administrator = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };
            EnsureSucceeded(await userManager.CreateAsync(administrator, password));
        }
        else if (!await userManager.CheckPasswordAsync(administrator, password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(administrator);
            EnsureSucceeded(await userManager.ResetPasswordAsync(administrator, token, password));
        }

        EnsureSucceeded(await userManager.SetLockoutEndDateAsync(administrator, null));
        EnsureSucceeded(await userManager.ResetAccessFailedCountAsync(administrator));

        if (!await userManager.IsInRoleAsync(administrator, AppRoles.Administrator))
            EnsureSucceeded(await userManager.AddToRoleAsync(administrator, AppRoles.Administrator));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
    }
}
