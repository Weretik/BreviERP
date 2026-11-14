namespace Identity.Infrastructure.Persistence.Seeding;

/// <summary>
/// The first administrator, created by the seed job. The password comes from the ADMIN_DEFAULT_PASSWORD secret.
/// </summary>
public sealed class AdminUserOptions
{
    public const string SectionName = "Identity:AdminUser";

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool LockoutEnabled { get; set; } = true;
}
