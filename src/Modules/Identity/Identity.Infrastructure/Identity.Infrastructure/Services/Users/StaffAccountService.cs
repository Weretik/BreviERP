using Ardalis.Result;
using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Users;

/// <summary>
/// Staff account rules: at most one director; only the director changes the director; nobody blocks or deletes
/// their own account; only the director changes their own role.
/// </summary>
public sealed class StaffAccountService(
    AppIdentityDbContext dbContext,
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IAvatarStorage avatarStorage,
    AvatarService avatarService,
    ILogger<StaffAccountService> logger) : IStaffAccountService
{
    public async Task<IReadOnlyList<StaffAccountDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var rows = await (
                from user in dbContext.Users
                join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
                join role in dbContext.Roles on userRole.RoleId equals role.Id
                where StaffRoles.All.Contains(role.Name!)
                orderby user.Email
                select new { user.Id, user.Email, user.FullName, user.AvatarPath, user.PhoneNumber, Role = role.Name!, user.LockoutEnd })
            .ToListAsync(cancellationToken);

        // A user with several staff roles (the director also holds Admin) is listed once, with the strongest role.
        return rows
            .GroupBy(x => x.Id)
            .Select(group => group.OrderBy(x => IndexOfRole(x.Role)).First())
            .Select(x => new StaffAccountDto(
                x.Id, x.Email ?? string.Empty, x.FullName, x.Role, x.LockoutEnd > now, avatarStorage.BuildUrl(x.AvatarPath), x.PhoneNumber))
            .ToList();
    }

    public async Task<Result<StaffAccountDto>> CreateAsync(
        string email,
        string fullName,
        string role,
        string password,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Conflict("A user with this email already exists.");
        }

        if (role == RoleNames.Director && await DirectorExistsAsync(exceptUserId: null))
        {
            return Result.Conflict(OneDirectorMessage);
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FullName = fullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return Invalid(created);
        }

        var assigned = await userManager.AddToRolesAsync(user, StaffRoles.GrantedRoles(role));
        if (!assigned.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Invalid(assigned);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} created with role {Role} by {ActorId}", user.Id, role, CurrentUserId());
        return Result.Success(new StaffAccountDto(user.Id, email, user.FullName, role, IsBlocked: false, PhoneNumber: user.PhoneNumber));
    }

    public async Task<Result> ChangeRoleAsync(int userId, string role, CancellationToken cancellationToken = default)
    {
        var isSelf = userId == CurrentUserId();
        if (isSelf && !ActorIsDirector())
        {
            return SelfChangeRejected("You cannot change your own role.");
        }

        var target = await FindStaffForChangeAsync(userId);
        if (!target.IsSuccess)
        {
            return Failure(target);
        }

        var user = target.Value;
        if (role == RoleNames.Director && await DirectorExistsAsync(exceptUserId: userId))
        {
            return Result.Conflict(OneDirectorMessage);
        }

        var currentRoles = (await userManager.GetRolesAsync(user)).Where(StaffRoles.IsStaffRole).ToArray();
        var removed = await userManager.RemoveFromRolesAsync(user, currentRoles);
        var added = removed.Succeeded ? await userManager.AddToRolesAsync(user, StaffRoles.GrantedRoles(role)) : removed;
        if (!added.Succeeded)
        {
            return Invalid(added);
        }

        // Ends cookie sessions at the next stamp check, so the new role applies everywhere.
        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("AUDIT: Staff account {UserId} role set to {Role} by {ActorId}", userId, role, CurrentUserId());
        return Result.Success();
    }

    public async Task<Result> SetBlockedAsync(int userId, bool blocked, CancellationToken cancellationToken = default)
    {
        if (blocked && userId == CurrentUserId())
        {
            return SelfChangeRejected("You cannot block your own account.");
        }

        var target = await FindStaffForChangeAsync(userId);
        if (!target.IsSuccess)
        {
            return Failure(target);
        }

        var user = target.Value;
        if (blocked)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            // Ends the user's cookie sessions at the next stamp check.
            await userManager.UpdateSecurityStampAsync(user);
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} blocked={Blocked} by {ActorId}", userId, blocked, CurrentUserId());
        return Result.Success();
    }

    public async Task<Result> UpdateDetailsAsync(
        int userId,
        string fullName,
        string email,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var target = await FindStaffForChangeAsync(userId);
        if (!target.IsSuccess)
        {
            return Failure(target);
        }

        var user = target.Value;
        var normalizedEmail = email.Trim();
        var owner = await userManager.FindByEmailAsync(normalizedEmail);
        if (owner is not null && owner.Id != user.Id)
        {
            return Result.Conflict("A user with this email already exists.");
        }

        var emailChanged = !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase);
        user.FullName = fullName.Trim();
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;
        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            return Invalid(updated);
        }

        // A new sign-in email ends the user's sessions, like a new password.
        if (emailChanged)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} details changed by {ActorId}", userId, CurrentUserId());
        return Result.Success();
    }

    public async Task<Result> SetPasswordAsync(int userId, string password, CancellationToken cancellationToken = default)
    {
        var target = await FindStaffForChangeAsync(userId);
        if (!target.IsSuccess)
        {
            return Failure(target);
        }

        // The reset changes the security stamp, so the user's other sessions end too.
        var user = target.Value;
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, resetToken, password);
        if (!reset.Succeeded)
        {
            return Invalid(reset);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} password set by {ActorId}", userId, CurrentUserId());
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId == CurrentUserId())
        {
            return SelfChangeRejected("You cannot delete your own account.");
        }

        var target = await FindStaffForChangeAsync(userId);
        if (!target.IsSuccess)
        {
            return Failure(target);
        }

        // The photo file goes first; a failure to delete it is only logged.
        await avatarService.RemoveAsync(target.Value, cancellationToken);
        var deleted = await userManager.DeleteAsync(target.Value);
        if (!deleted.Succeeded)
        {
            return Invalid(deleted);
        }

        logger.LogInformation("AUDIT: Staff account {UserId} deleted by {ActorId}", userId, CurrentUserId());
        return Result.Success();
    }

    private const string OneDirectorMessage = "There can be only one director.";

    /// <summary>Finds a staff account the current user may change: only the director changes the director.</summary>
    private async Task<Result<AppUser>> FindStaffForChangeAsync(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        if (user is null)
        {
            return Result.NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Any(StaffRoles.IsStaffRole))
        {
            return Result.NotFound();
        }

        if (roles.Contains(RoleNames.Director) && !ActorIsDirector())
        {
            return Result.Forbidden("Only the director can change the director.");
        }

        return Result.Success(user);
    }

    private async Task<bool> DirectorExistsAsync(int? exceptUserId)
    {
        var directors = await userManager.GetUsersInRoleAsync(RoleNames.Director);
        return directors.Any(user => user.Id != exceptUserId);
    }

    private bool ActorIsDirector() =>
        httpContextAccessor.HttpContext?.User.IsInRole(RoleNames.Director) ?? false;

    private int? CurrentUserId() =>
        int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static int IndexOfRole(string role)
    {
        for (var i = 0; i < StaffRoles.All.Count; i++)
        {
            if (StaffRoles.All[i] == role)
            {
                return i;
            }
        }

        return int.MaxValue;
    }

    private static Result Failure(Result<AppUser> target) =>
        target.Status == ResultStatus.Forbidden ? Result.Forbidden(target.Errors.ToArray()) : Result.NotFound();

    private static Result SelfChangeRejected(string message) =>
        Result.Invalid(new ValidationError(message));

    private static Result Invalid(IdentityResult result) =>
        Result.Invalid(result.Errors.Select(error => new ValidationError(error.Description)).ToList());
}
