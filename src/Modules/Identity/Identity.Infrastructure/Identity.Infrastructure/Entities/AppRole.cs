namespace Identity.Infrastructure.Entities;

public class AppRole : IdentityRole<int>
{
    /// <summary>Scope of a role made on «Права доступу»; its <see cref="Description"/> is the name people see.</summary>
    public const string CustomScope = "custom";

    public string Description { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    public bool IsSystemRole { get; set; } = false;
    public int AccessLevel { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
