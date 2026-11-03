using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Services;

/// <summary>
/// Staff accounts (director, administrators, accountants, managers). The current user is the acting administrator.
/// Only the director changes the director; there is at most one director.
/// </summary>
public interface IStaffAccountService
{
    Task<IReadOnlyList<StaffAccountDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<Result<StaffAccountDto>> CreateAsync(string email, string fullName, string role, string password, CancellationToken cancellationToken = default);
    Task<Result> ChangeRoleAsync(int userId, string role, CancellationToken cancellationToken = default);
    Task<Result> SetBlockedAsync(int userId, bool blocked, CancellationToken cancellationToken = default);
    Task<Result> SetPasswordAsync(int userId, string password, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, CancellationToken cancellationToken = default);
}
