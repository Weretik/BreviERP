using Identity.Application.Features.Access.Shared;
using Identity.Domain.Authorization;

namespace Identity.Api.Contracts.Access;

/// <summary>Actions as the admin writes them: <c>view</c>, <c>create</c>, <c>edit</c>, <c>delete</c>.</summary>
public static class AccessActionNames
{
    public static string Of(AccessAction action) => action.ToString().ToLowerInvariant();

    public static IReadOnlyList<string> Of(IEnumerable<AccessAction> actions) => actions.Select(Of).ToList();

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Of(IReadOnlyDictionary<string, IReadOnlyList<AccessAction>> grants) =>
        grants.ToDictionary(x => x.Key, x => Of(x.Value), StringComparer.Ordinal);

    /// <summary>Reads the admin's names; null when one is not an action.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<AccessAction>>? Parse(IReadOnlyDictionary<string, IReadOnlyList<string>>? grants)
    {
        var result = new Dictionary<string, IReadOnlyList<AccessAction>>(StringComparer.Ordinal);
        foreach (var (area, names) in grants ?? new Dictionary<string, IReadOnlyList<string>>())
        {
            var actions = new List<AccessAction>();
            foreach (var name in names ?? [])
            {
                var action = AccessActions.Ordered.FirstOrDefault(x => Of(x) == name);
                if (action == AccessAction.None) return null;
                actions.Add(action);
            }

            result[area] = actions.Distinct().ToList();
        }

        return result;
    }
}

public sealed record AccessAreaResponse(string Key, string Group, string Title, IReadOnlyList<string> Actions);

/// <param name="Name">The role's key: a built-in name such as <c>Manager</c> or a generated <c>custom-…</c>.</param>
/// <param name="Locked">The director: always every right, not editable.</param>
/// <param name="BuiltIn">Known to the code: never deleted.</param>
/// <param name="Version">Send it back on save; 0 until the first save.</param>
/// <param name="Grants">Page key → allowed actions; a page left out is hidden from the role.</param>
public sealed record AccessRoleResponse(
    string Name,
    string Title,
    bool Locked,
    bool BuiltIn,
    int UserCount,
    int Version,
    string? UpdatedByName,
    DateTimeOffset? UpdatedAt,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Grants)
{
    public static AccessRoleResponse From(AccessRoleDto dto) =>
        new(dto.Name, dto.Title, dto.Locked, dto.BuiltIn, dto.UserCount, dto.Version, dto.UpdatedByName, dto.UpdatedAt,
            AccessActionNames.Of(dto.Grants));
}

public sealed record AccessRightsResponse(IReadOnlyList<AccessAreaResponse> Areas, IReadOnlyList<AccessRoleResponse> Roles)
{
    public static AccessRightsResponse From(AccessRightsDto dto) =>
        new(dto.Areas.Select(x => new AccessAreaResponse(x.Key, x.Group, x.Title, AccessActionNames.Of(x.Actions))).ToList(),
            dto.Roles.Select(AccessRoleResponse.From).ToList());
}

/// <param name="Version">The version the page was loaded with; 409 when someone saved in between.</param>
/// <param name="Grants">Page key → actions (<c>view</c>, <c>create</c>, <c>edit</c>, <c>delete</c>); «view» comes first.</param>
public sealed record SaveRoleAccessRequest(int Version, IReadOnlyDictionary<string, IReadOnlyList<string>> Grants);

public sealed record CreateRoleRequest(string Title);

public sealed record RoleTitleResponse(string Name, string Title, bool BuiltIn);

/// <param name="AllowAll">The director: every page and action, even ones added later.</param>
public sealed record MyAccessResponse(
    bool AllowAll,
    IReadOnlyList<RoleTitleResponse> Roles,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Grants)
{
    public static MyAccessResponse From(MyAccessDto dto) =>
        new(dto.AllowAll, dto.Roles.Select(x => new RoleTitleResponse(x.Name, x.Title, x.BuiltIn)).ToList(),
            AccessActionNames.Of(dto.Grants));
}
