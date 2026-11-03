using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Services;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Sessions;

public sealed class CookieSessionService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    ILogger<CookieSessionService> logger) : ICookieSessionService
{
    public async Task<SessionMeResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            logger.LogWarning("AUDIT: Cookie login failed: user not found for email {Email}", email);
            return null;
        }

        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("AUDIT: Cookie login failed for userId {UserId}; locked out: {IsLockedOut}", user.Id, result.IsLockedOut);
            return null;
        }

        logger.LogInformation("AUDIT: Cookie login success for userId {UserId}", user.Id);
        return await ToResultAsync(user);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signInManager.SignOutAsync();
    }

    public async Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var principal = httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return null;
        }

        var user = await userManager.GetUserAsync(principal);
        return user is null ? null : await ToResultAsync(user);
    }

    private async Task<SessionMeResult> ToResultAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new SessionMeResult(user.Id.ToString(CultureInfo.InvariantCulture), user.Email ?? string.Empty, user.FullName, roles.ToArray());
    }
}
