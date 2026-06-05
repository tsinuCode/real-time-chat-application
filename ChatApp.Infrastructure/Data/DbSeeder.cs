using ChatApp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        if (await userManager.FindByNameAsync("demo") is null)
        {
            var demoUser = new ApplicationUser
            {
                UserName = "demo",
                Email = "demo@chatapp.local",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(demoUser, "Demo@123");
            await userManager.AddToRoleAsync(demoUser, "User");
        }

        if (await userManager.FindByNameAsync("alice") is null)
        {
            var alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "alice@chatapp.local",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(alice, "Alice@123");
            await userManager.AddToRoleAsync(alice, "User");
        }
    }
}
