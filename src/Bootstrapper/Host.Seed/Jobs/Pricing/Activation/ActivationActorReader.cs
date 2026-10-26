using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace Host.Seed.Jobs;

internal static class ActivationActorReader
{
    public static async Task<string> RequireManagerAsync(UserManager<AppUser> users, string id)
    {
        var user = await users.FindByIdAsync(id);
        if (user is null || await users.IsLockedOutAsync(user)
            || !(await users.IsInRoleAsync(user, "Admin") || await users.IsInRoleAsync(user, "Manager")))
            throw new InvalidOperationException("An existing unlocked Admin/Manager actor is required.");
        return user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
