using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Host.Api.Security.AnonymousApiAccess;

public sealed class IdentityAnonymousApiActorResolver(
    UserManager<AppUser> users,
    IOptions<AnonymousApiAccessOptions> options,
    TimeProvider time) : IAnonymousApiActorResolver
{
    public Task<AnonymousApiActor?> ResolveAsync(CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var selectedId = options.Value.ActorUserId;
        return users.Users.AsNoTracking()
            .Where(user => (!selectedId.HasValue || user.Id == selectedId.Value) &&
                (!user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= now) &&
                user.UserRoles.Any(role => role.Role.Name == "Admin"))
            .OrderBy(user => user.Id)
            .Select(user => new AnonymousApiActor(user.Id, user.FullName, user.Email ?? string.Empty))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
