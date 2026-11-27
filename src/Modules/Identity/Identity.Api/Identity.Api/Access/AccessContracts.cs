using Identity.Application.Features.Access.Shared;
using Identity.Domain.Authorization;

namespace Identity.Api.Access;

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

/// <param name="ActionLabels">Action (<c>view</c>, <c>create</c>...) → what it means on this page, in Ukrainian; one per action
/// the page has.</param>
public sealed record AccessAreaResponse(string Key, string Group, string Title, IReadOnlyList<string> Actions,
    IReadOnlyDictionary<string, string> ActionLabels)
{
    public static AccessAreaResponse From(AccessAreaDto dto) =>
        new(dto.Key, dto.Group, dto.Title, AccessActionNames.Of(dto.Actions),
            dto.Labels.ToDictionary(x => AccessActionNames.Of(x.Key), x => x.Value, StringComparer.Ordinal));
}

/// <summary>A page or action only Admin and the director may use; shown without switches.</summary>
public sealed record AdminOnlyItemResponse(string Group, string Title, string Reason);

/// <param name="Name">The role's key: a built-in name such as <c>Manager</c> or a generated <c>custom-…</c>.</param>
/// <param name="Locked">The director: always every right, not editable.</param>
/// <param name="BuiltIn">Known to the code: never deleted.</param>
/// <param name="UserNames">Who holds the role, by name, alphabetically.</param>
/// <param name="Version">Send it back on save; 0 until the first save.</param>
/// <param name="Grants">Page key → allowed actions; a page left out is hidden from the role.</param>
public sealed record AccessRoleResponse(
    string Name,
    string Title,
    bool Locked,
    bool BuiltIn,
    int UserCount,
    IReadOnlyList<string> UserNames,
    int Version,
    string? UpdatedByName,
    DateTimeOffset? UpdatedAt,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Grants)
{
    public static AccessRoleResponse From(AccessRoleDto dto) =>
        new(dto.Name, dto.Title, dto.Locked, dto.BuiltIn, dto.UserCount, dto.UserNames, dto.Version, dto.UpdatedByName, dto.UpdatedAt,
            AccessActionNames.Of(dto.Grants));
}

/// <param name="AdminOnly">Pages and actions only Admin may use, for the block without switches.</param>
public sealed record AccessRightsResponse(IReadOnlyList<AccessAreaResponse> Areas, IReadOnlyList<AccessRoleResponse> Roles,
    IReadOnlyList<AdminOnlyItemResponse> AdminOnly)
{
    public static AccessRightsResponse From(AccessRightsDto dto) =>
        new(dto.Areas.Select(AccessAreaResponse.From).ToList(),
            dto.Roles.Select(AccessRoleResponse.From).ToList(),
            dto.AdminOnly.Select(x => new AdminOnlyItemResponse(x.Group, x.Title, x.Reason)).ToList());
}

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
