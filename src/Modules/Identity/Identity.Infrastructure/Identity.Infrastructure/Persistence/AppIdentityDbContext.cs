using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Identity.Infrastructure.Persistence.Access;
using Identity.Infrastructure.Persistence.Users;

namespace Identity.Infrastructure.Persistence;

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>(options), IDataProtectionKeyContext
{
    // Data Protection keys sign the session cookie; shared by every Cloud Run instance.
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    // «Права доступу»: the saved rights of each role.
    public DbSet<RoleAccess> RoleAccess => Set<RoleAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identity");
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(
            typeof(AppIdentityDbContext).Assembly,
            type => type.Namespace?.StartsWith("Identity.Infrastructure.Persistence") ?? false);
    }
}
