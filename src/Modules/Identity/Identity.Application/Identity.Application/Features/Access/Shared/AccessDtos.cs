using Identity.Domain.Authorization;

namespace Identity.Application.Features.Access.Shared;

public sealed record AccessAreaDto(string Key, string Group, string Title, IReadOnlyList<AccessAction> Actions);

/// <param name="Name">The role as Identity stores it; a new role gets a generated name.</param>
/// <param name="Title">What people see, such as «Менеджер» or «Технолог».</param>
/// <param name="Locked">The director: rights are not edited.</param>
/// <param name="BuiltIn">A role the code knows: never deleted.</param>
/// <param name="Version">0 until the role's rights are saved; then the save's version, for 409 on a stale save.</param>
public sealed record AccessRoleDto(
    string Name,
    string Title,
    bool Locked,
    bool BuiltIn,
    int UserCount,
    int Version,
    string? UpdatedByName,
    DateTimeOffset? UpdatedAt,
    IReadOnlyDictionary<string, IReadOnlyList<AccessAction>> Grants);

public sealed record AccessRightsDto(IReadOnlyList<AccessAreaDto> Areas, IReadOnlyList<AccessRoleDto> Roles);

/// <summary>The signed-in user's rights: the director may do everything; role titles for the shell.</summary>
public sealed record MyAccessDto(bool AllowAll, IReadOnlyList<RoleTitleDto> Roles,
    IReadOnlyDictionary<string, IReadOnlyList<AccessAction>> Grants);

public sealed record RoleTitleDto(string Name, string Title, bool BuiltIn);

public static class AccessActions
{
    public static readonly AccessAction[] Ordered = [AccessAction.View, AccessAction.Create, AccessAction.Edit, AccessAction.Delete];

    public static IReadOnlyList<AccessAction> Split(AccessAction actions) =>
        Ordered.Where(x => actions.HasFlag(x)).ToArray();

    public static AccessAction Join(IEnumerable<AccessAction> actions) =>
        actions.Aggregate(AccessAction.None, (all, x) => all | x);

    public static IReadOnlyDictionary<string, IReadOnlyList<AccessAction>> Split(IReadOnlyDictionary<string, AccessAction> grants) =>
        grants.Where(x => x.Value != AccessAction.None)
            .ToDictionary(x => x.Key, x => Split(x.Value), StringComparer.Ordinal);
}
