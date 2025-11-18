using Domain.Identity.Constants;
using Infrastructure.Common.Contracts;
using Infrastructure.Identity.Entities;

namespace Infrastructure.Identity.Seeders;

public sealed class RoleSeeder(
    RoleManager<AppRole> roleManager,
    ILogger<RoleSeeder> logger)
    : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roles = new List<AppRole>
        {
            new() { Name = AppRoles.Admin, Description = "Адміністратор системи", Scope = "system", IsSystemRole = true, AccessLevel = 100 },
            new() { Name = AppRoles.Manager, Description = "Контент-менеджер", Scope = "content", IsSystemRole = true, AccessLevel = 50 },
            new() { Name = AppRoles.User, Description = "Звичайний користувач", Scope = "user", IsSystemRole = true, AccessLevel = 10 }
        };

        foreach (var role in roles)
        {
            if (role.Name != null)
            {
                var exists = await roleManager.FindByNameAsync(role.Name);
                if (exists != null) continue;
            }

            role.NormalizedName = role.Name?.ToUpperInvariant();

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Помилка при створенні ролі {Role}: {Errors}", role.Name, errors);
            }
            else
            {
                logger.LogInformation("Роль {Role} успішно створена", role.Name);
            }
        }
    }
}
