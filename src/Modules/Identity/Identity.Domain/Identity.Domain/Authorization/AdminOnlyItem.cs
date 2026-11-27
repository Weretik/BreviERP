namespace Identity.Domain.Authorization;

/// <summary>
/// A page or action of the admin that only Admin (and the director, who holds Admin) may use, on purpose (ACC-002 R-04).
/// «Права доступу» shows these without switches, so the director sees the whole admin.
/// </summary>
public sealed record AdminOnlyItem(string Group, string Title, string Reason);
