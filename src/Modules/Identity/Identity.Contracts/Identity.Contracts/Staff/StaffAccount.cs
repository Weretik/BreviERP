namespace Identity.Contracts.Staff;

/// <summary>A staff account as other modules see it: name, contacts, block state and roles.</summary>
public sealed record StaffAccount(
    int Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    bool IsLockedOut,
    IReadOnlyList<string> Roles)
{
    /// <summary>The full name, else the e-mail, else «Користувач #id».</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Email ?? $"Користувач #{Id}" : FullName;

    public bool IsInAnyRole(params string[] roles) =>
        Roles.Any(role => roles.Contains(role, StringComparer.OrdinalIgnoreCase));
}
