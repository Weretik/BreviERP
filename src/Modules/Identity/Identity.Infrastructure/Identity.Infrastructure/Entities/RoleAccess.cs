namespace Identity.Infrastructure.Entities;

/// <summary>
/// The saved rights of one role on «Права доступу»: page key → allowed actions (flags), as JSON.
/// A role without a row keeps the catalogue defaults.
/// </summary>
public class RoleAccess
{
    public int RoleId { get; set; }
    public string Grants { get; set; } = "{}";
    public int Version { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public AppRole Role { get; set; } = null!;
}
