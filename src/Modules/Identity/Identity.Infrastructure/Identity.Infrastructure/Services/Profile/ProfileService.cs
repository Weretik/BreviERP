using Ardalis.Result;
using Identity.Application.Features.Avatars;
using Identity.Application.Features.Profile.Shared;
using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Profile;

/// <summary>
/// The signed-in staff account's own profile. Controller policies decide who may call what;
/// email and password changes additionally require the current password.
/// </summary>
public sealed class ProfileService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AvatarService avatarService,
    IAvatarStorage avatarStorage,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ProfileService> logger) : IProfileService
{
    public const string WrongPasswordMessage = "The current password is incorrect.";
    public const string EmailTakenMessage = "A user with this email already exists.";

    public async Task<Result<ProfileDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result<ProfileDto>.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        // A role made on «Права доступу» comes after the built-in ones.
        var role = StaffRoles.All.FirstOrDefault(roles.Contains)
            ?? roles.FirstOrDefault(x => !AccessCatalog.IsOutsider(x))
            ?? string.Empty;
        return Result.Success(new ProfileDto(user.Id, user.Email ?? string.Empty, user.FullName, role, avatarStorage.BuildUrl(user.AvatarPath), user.PhoneNumber));
    }

    public async Task<Result> ChangeNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        user.FullName = fullName.Trim();
        var updated = await userManager.UpdateAsync(user);
        return updated.Succeeded ? Result.Success() : Invalid(updated);
    }

    public async Task<Result> ChangePhoneAsync(string? phoneNumber, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        var updated = await userManager.UpdateAsync(user);
        return updated.Succeeded ? Result.Success() : Invalid(updated);
    }

    public async Task<Result<AvatarResult>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        return user is null
            ? Result<AvatarResult>.Unauthorized()
            : await avatarService.ReplaceAsync(user, upload, cancellationToken);
    }

    public async Task<Result> RemoveAvatarAsync(CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        return user is null ? Result.Unauthorized() : await avatarService.RemoveAsync(user, cancellationToken);
    }

    public async Task<Result> ChangeEmailAsync(string email, string currentPassword, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!await IsCurrentPasswordAsync(user, currentPassword))
        {
            return WrongPassword();
        }

        var normalized = email.Trim();
        var owner = await userManager.FindByEmailAsync(normalized);
        if (owner is not null && owner.Id != user.Id)
        {
            return Result.Conflict(EmailTakenMessage);
        }

        user.Email = normalized;
        user.UserName = normalized;
        user.EmailConfirmed = true;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            return Invalid(updated);
        }

        await userManager.UpdateSecurityStampAsync(user);
        await signInManager.RefreshSignInAsync(user);
        logger.LogInformation("AUDIT: User {UserId} changed own email", user.Id);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!await IsCurrentPasswordAsync(user, currentPassword))
        {
            return WrongPassword();
        }

        var changed = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changed.Succeeded)
        {
            return Invalid(changed);
        }

        // The security stamp changed, which ends other sessions; this browser gets a fresh cookie.
        await signInManager.RefreshSignInAsync(user);
        logger.LogInformation("AUDIT: User {UserId} changed own password", user.Id);
        return Result.Success();
    }

    // Counts a wrong password towards lockout, like a failed sign-in.
    private async Task<bool> IsCurrentPasswordAsync(AppUser user, string password) =>
        (await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)).Succeeded;

    private async Task<AppUser?> CurrentUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        return principal is null ? null : await userManager.GetUserAsync(principal);
    }

    private static Result WrongPassword() =>
        Result.Invalid(new ValidationError("currentPassword", WrongPasswordMessage, null, ValidationSeverity.Error));

    private static Result Invalid(IdentityResult result) =>
        Result.Invalid(result.Errors.Select(error => new ValidationError(error.Description)).ToList());
}
