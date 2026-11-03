using Identity.Domain.Constants;
using Identity.Infrastructure.Contracts;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Seeders;

/// <summary>
/// Creates the configured administrator if that email has no account yet. The password comes from the
/// ADMIN_DEFAULT_PASSWORD secret; without it nothing is created. An existing administrator only gets the
/// configured lockout setting, its password is never changed here.
/// </summary>
public class IdentitySeeder(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    IOptions<AdminUserOptions> adminOptions,
    ILogger<IdentitySeeder> logger)
    : IIdentitySeeder
{
    private readonly AdminUserOptions _adminOptions = adminOptions.Value;

    public async Task SeedAsync(IServiceProvider _, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(_adminOptions.Email))
        {
            logger.LogWarning("Identity:AdminUser:Email is not configured. Admin seeding is skipped.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(_adminOptions.Email);
        if (existingAdmin != null)
        {
            await AlignLockoutAsync(existingAdmin);
            logger.LogInformation("Admin user already exists: {Email}", _adminOptions.Email);
            return;
        }

        var password = configuration["ADMIN_DEFAULT_PASSWORD"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogError("ADMIN_DEFAULT_PASSWORD is not configured. Admin seeding is skipped.");
            return;
        }

        var user = new AppUser
        {
            UserName = _adminOptions.Email,
            Email = _adminOptions.Email,
            FullName = _adminOptions.FullName,
            EmailConfirmed = true,
            LockoutEnabled = _adminOptions.LockoutEnabled
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}", Describe(result));
            return;
        }

        result = await userManager.AddToRoleAsync(user, AppRoles.Admin);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to assign admin role: {Errors}", Describe(result));
        }

        logger.LogInformation("Admin user created: {Email}", user.Email);
    }

    private async Task AlignLockoutAsync(AppUser admin)
    {
        if (admin.LockoutEnabled == _adminOptions.LockoutEnabled)
        {
            return;
        }

        var result = await userManager.SetLockoutEnabledAsync(admin, _adminOptions.LockoutEnabled);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to align admin lockout: {Errors}", Describe(result));
        }
    }

    private static string Describe(IdentityResult result) =>
        string.Join(", ", result.Errors.Select(e => e.Description));
}
