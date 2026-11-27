namespace Identity.Domain.Authorization;

/// <summary>What a role may do on a page of «Права доступу».</summary>
[Flags]
public enum AccessAction
{
    None = 0,
    View = 1,
    Create = 2,
    Edit = 4,
    Delete = 8
}

/// <summary>
/// A page (area) of the admin whose API is guarded by «Права доступу». <paramref name="Defaults"/> are the rights the built-in
/// roles had before the page existed (the admin menu they saw), so nothing changes until someone saves other rights.
/// With <paramref name="DefaultsForSavedRoles"/> a role saved before the page existed also gets the page's defaults, and a
/// save keeps the page even when it is unticked, so the director's choice wins from then on (DESK-001).
/// <paramref name="Labels"/> say what each action does on this page (ACC-002 R-05).
/// </summary>
public sealed record AccessArea(
    string Key,
    string Group,
    string Title,
    AccessAction Actions,
    IReadOnlyDictionary<string, AccessAction> Defaults,
    bool DefaultsForSavedRoles = false,
    AccessActionLabels Labels = default)
{
    /// <summary>The Ukrainian name of <paramref name="action"/> on this page; null when the page does not have it.</summary>
    public string? LabelOf(AccessAction action) => Actions.HasFlag(action) ? Labels.For(action) : null;
}
