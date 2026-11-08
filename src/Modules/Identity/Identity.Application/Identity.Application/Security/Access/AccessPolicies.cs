using Identity.Domain.Authorization;

namespace Identity.Application.Security.Access;

/// <summary>
/// Policy names checked against «Права доступу»: <c>access:quotes</c> takes the action from the HTTP method,
/// <c>access:quotes:delete</c> names it. Modules write the same strings in their own access classes.
/// </summary>
public static class AccessPolicies
{
    public const string Prefix = "access:";

    public static string For(string area) => Prefix + area;

    public static string For(string area, AccessAction action) => $"{Prefix}{area}:{action.ToString().ToLowerInvariant()}";

    /// <summary>Reads <c>access:area[:action]</c>; null when the name is not one or names an unknown page or action.</summary>
    public static (AccessArea Area, AccessAction? Action)? Parse(string policyName)
    {
        if (!policyName.StartsWith(Prefix, StringComparison.Ordinal)) return null;
        var parts = policyName[Prefix.Length..].Split(':');
        var area = AccessCatalog.Find(parts[0]);
        if (area is null || parts.Length > 2) return null;
        if (parts.Length == 1) return (area, null);
        return Enum.TryParse<AccessAction>(parts[1], ignoreCase: true, out var action) && action != AccessAction.None
            ? (area, action)
            : null;
    }
}
