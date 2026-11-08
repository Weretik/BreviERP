using Identity.Domain.Authorization;

namespace Identity.Application.Security.Access;

/// <summary>The rights of the given roles together, by page; a built-in role nobody saved keeps its old rules.</summary>
public interface IAccessGrantSource
{
    Task<IReadOnlyDictionary<string, AccessAction>> GrantsOfAsync(IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
