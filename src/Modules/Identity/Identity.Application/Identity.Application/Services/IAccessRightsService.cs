using Identity.Application.Features.Access.Shared;
using Identity.Domain.Authorization;

namespace Identity.Application.Services;

/// <summary>
/// «Права доступу»: roles with their rights by page. The director's rights are fixed; built-in roles are never deleted;
/// a role with users is not deleted. Every save drops the cached rights at once.
/// </summary>
public interface IAccessRightsService
{
    Task<AccessRightsDto> GetAsync(CancellationToken cancellationToken = default);
    Task<Result<AccessRoleDto>> SaveAsync(string role, int version, IReadOnlyDictionary<string, AccessAction> grants,
        CancellationToken cancellationToken = default);
    Task<Result<AccessRoleDto>> CreateRoleAsync(string title, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(string role, CancellationToken cancellationToken = default);

    /// <summary>The signed-in user's rights, for the menu and buttons of the admin.</summary>
    Task<MyAccessDto> GetMineAsync(CancellationToken cancellationToken = default);
}
