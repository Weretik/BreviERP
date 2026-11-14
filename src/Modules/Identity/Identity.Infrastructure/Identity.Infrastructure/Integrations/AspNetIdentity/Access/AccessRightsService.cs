using Ardalis.Result;
using Identity.Application.Features.Access.Shared;
using Identity.Application.Security.Access;
using Identity.Domain.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Access;
using Identity.Infrastructure.Persistence.Users;
using Identity.Domain.Authorization.Roles;

namespace Identity.Infrastructure.Integrations.AspNetIdentity.Access;

/// <summary>
/// «Права доступу» (client corrections item 11). The director always has everything; built-in roles keep their old
/// rules until someone saves other rights; a new role starts with nothing; a role with users is not deleted.
/// </summary>
public sealed class AccessRightsService(
    AppIdentityDbContext dbContext,
    RoleManager<AppRole> roleManager,
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IAccessGrantSource grantSource,
    IMemoryCache cache,
    ILogger<AccessRightsService> logger) : IAccessRightsService
{
    public const string StaleMessage = "Someone else changed this role's rights. Reload the page.";
    public const string HasUsersMessage = "The role has users. Give them another role first.";
    public const string TitleTakenMessage = "A role with this name already exists.";

    public async Task<AccessRightsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var roles = await StaffRolesAsync(cancellationToken);
        var counts = await UserCountsAsync(roles.Select(x => x.Name!).ToArray(), cancellationToken);
        var saved = await dbContext.RoleAccess.AsNoTracking().ToDictionaryAsync(x => x.RoleId, cancellationToken);
        var areas = AccessCatalog.Areas
            .Select(x => new AccessAreaDto(x.Key, x.Group, x.Title, AccessActions.Split(x.Actions)))
            .ToList();
        return new AccessRightsDto(areas, roles
            .Select(role => ToDto(role, counts.GetValueOrDefault(role.Name!), saved.GetValueOrDefault(role.Id)))
            .ToList());
    }

    public async Task<Result<AccessRoleDto>> SaveAsync(
        string role,
        int version,
        IReadOnlyDictionary<string, AccessAction> grants,
        CancellationToken cancellationToken = default)
    {
        if (AccessCatalog.IsLocked(role))
        {
            return Result.Invalid(new ValidationError("Директор завжди має всі права."));
        }

        var appRole = await FindEditableAsync(role);
        if (appRole is null)
        {
            return Result.NotFound();
        }

        var row = await dbContext.RoleAccess.FirstOrDefaultAsync(x => x.RoleId == appRole.Id, cancellationToken);
        if ((row?.Version ?? 0) != version)
        {
            return Result.Conflict(StaleMessage);
        }

        if (row is null)
        {
            row = new RoleAccess { RoleId = appRole.Id };
            dbContext.RoleAccess.Add(row);
        }

        var masked = grants
            .Select(x => (x.Key, Actions: x.Value & (AccessCatalog.Find(x.Key)?.Actions ?? AccessAction.None)))
            .Where(x => x.Actions != AccessAction.None)
            .ToDictionary(x => x.Key, x => x.Actions, StringComparer.Ordinal);
        row.Grants = AccessGrantSource.Serialize(masked);
        row.Version = version + 1;
        row.UpdatedByName = await ActorNameAsync();
        row.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another save of the same role won the race (update: concurrency token; first save: primary key).
            return Result.Conflict(StaleMessage);
        }

        cache.Remove(AccessGrantSource.CacheKey);
        logger.LogInformation("AUDIT: Access rights of role {Role} saved (version {Version}) by {Actor}", role, row.Version, row.UpdatedByName);
        var counts = await UserCountsAsync([appRole.Name!], cancellationToken);
        return Result.Success(ToDto(appRole, counts.GetValueOrDefault(appRole.Name!), row));
    }

    public async Task<Result<AccessRoleDto>> CreateRoleAsync(string title, CancellationToken cancellationToken = default)
    {
        var roles = await StaffRolesAsync(cancellationToken);
        if (roles.Any(x => string.Equals(TitleOf(x), title, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Conflict(TitleTakenMessage);
        }

        var role = new AppRole
        {
            Name = "custom-" + Guid.NewGuid().ToString("N")[..12],
            Description = title,
            Scope = AppRole.CustomScope,
            IsSystemRole = false,
            AccessLevel = 1
        };
        var created = await roleManager.CreateAsync(role);
        if (!created.Succeeded)
        {
            return Result.Invalid(created.Errors.Select(x => new ValidationError(x.Description)).ToList());
        }

        logger.LogInformation("AUDIT: Role {Role} «{Title}» created by {Actor}", role.Name, title, await ActorNameAsync());
        return Result.Success(ToDto(role, 0, null));
    }

    public async Task<Result> DeleteRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        if (AccessCatalog.IsBuiltIn(role))
        {
            return Result.Invalid(new ValidationError("Вбудовану роль видалити не можна."));
        }

        var appRole = await roleManager.FindByNameAsync(role);
        if (appRole is null || appRole.Scope != AppRole.CustomScope)
        {
            return Result.NotFound();
        }

        if (await dbContext.UserRoles.AnyAsync(x => x.RoleId == appRole.Id, cancellationToken))
        {
            return Result.Conflict(HasUsersMessage);
        }

        var deleted = await roleManager.DeleteAsync(appRole);
        if (!deleted.Succeeded)
        {
            return Result.Invalid(deleted.Errors.Select(x => new ValidationError(x.Description)).ToList());
        }

        cache.Remove(AccessGrantSource.CacheKey);
        logger.LogInformation("AUDIT: Role {Role} «{Title}» deleted by {Actor}", role, appRole.Description, await ActorNameAsync());
        return Result.Success();
    }

    public async Task<MyAccessDto> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var user = httpContextAccessor.HttpContext?.User;
        IReadOnlyCollection<string> roleNames = user is null ? [] : AccessClaims.RolesOf(user);
        var allowAll = roleNames.Any(AccessCatalog.IsLocked);
        var grants = allowAll ? AccessCatalog.Everything() : await grantSource.GrantsOfAsync(roleNames, cancellationToken);

        var staff = roleNames.Where(x => !AccessCatalog.IsOutsider(x)).ToArray();
        var custom = await dbContext.Roles.AsNoTracking()
            .Where(x => x.Scope == AppRole.CustomScope && staff.Contains(x.Name!))
            .ToDictionaryAsync(x => x.Name!, x => x.Description, cancellationToken);
        var titles = staff
            .Where(x => AccessCatalog.IsBuiltIn(x) || custom.ContainsKey(x))
            .Select(x => new RoleTitleDto(x, AccessCatalog.BuiltInTitles.GetValueOrDefault(x) ?? custom[x], AccessCatalog.IsBuiltIn(x)))
            .ToList();
        return new MyAccessDto(allowAll, titles, AccessActions.Split(grants));
    }

    /// <summary>Built-in staff roles in their fixed order, then new roles by name.</summary>
    private async Task<IReadOnlyList<AppRole>> StaffRolesAsync(CancellationToken cancellationToken)
    {
        var builtIn = AccessCatalog.BuiltInRoles.ToArray();
        var rows = await dbContext.Roles.AsNoTracking()
            .Where(x => builtIn.Contains(x.Name!) || x.Scope == AppRole.CustomScope)
            .ToListAsync(cancellationToken);
        return rows
            .OrderBy(x => AccessCatalog.IsBuiltIn(x.Name!) ? Array.IndexOf(builtIn, x.Name!) : builtIn.Length)
            .ThenBy(x => x.Description, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>People per role; the director also holds Admin but is counted only as the director.</summary>
    private async Task<IReadOnlyDictionary<string, int>> UserCountsAsync(string[] roleNames, CancellationToken cancellationToken)
    {
        var pairs = await (
                from userRole in dbContext.UserRoles
                join role in dbContext.Roles on userRole.RoleId equals role.Id
                where roleNames.Contains(role.Name!) || role.Name == RoleNames.Director
                select new { userRole.UserId, Role = role.Name! })
            .ToListAsync(cancellationToken);
        var directors = pairs.Where(x => x.Role == RoleNames.Director).Select(x => x.UserId).ToHashSet();
        return pairs
            .Where(x => roleNames.Contains(x.Role))
            .Where(x => x.Role != RoleNames.Admin || !directors.Contains(x.UserId))
            .GroupBy(x => x.Role)
            .ToDictionary(x => x.Key, x => x.Select(y => y.UserId).Distinct().Count(), StringComparer.Ordinal);
    }

    private async Task<AppRole?> FindEditableAsync(string role)
    {
        var appRole = await roleManager.FindByNameAsync(role);
        return appRole is not null && (AccessCatalog.IsBuiltIn(role) || appRole.Scope == AppRole.CustomScope) ? appRole : null;
    }

    private static AccessRoleDto ToDto(AppRole role, int userCount, RoleAccess? saved)
    {
        var name = role.Name!;
        var grants = AccessCatalog.IsLocked(name) ? AccessCatalog.Everything()
            : saved is not null ? AccessGrantSource.Parse(saved.Grants, name)
            : AccessCatalog.DefaultsOf(name);
        return new AccessRoleDto(
            name,
            TitleOf(role),
            AccessCatalog.IsLocked(name),
            AccessCatalog.IsBuiltIn(name),
            userCount,
            saved?.Version ?? 0,
            saved?.UpdatedByName,
            saved?.UpdatedAt,
            AccessActions.Split(grants));
    }

    private static string TitleOf(AppRole role) =>
        AccessCatalog.BuiltInTitles.GetValueOrDefault(role.Name!) ?? role.Description;

    private async Task<string?> ActorNameAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal is null) return null;
        var user = await userManager.GetUserAsync(principal);
        return user is null ? null : string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
    }
}
