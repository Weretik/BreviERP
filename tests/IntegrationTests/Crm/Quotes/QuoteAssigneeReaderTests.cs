using Host.Api.Crm;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteAssigneeReaderTests : CrmDatabaseFixture
{
    [Test]
    public async Task SearchAndEligibilityExcludeLockedAndNonManagerUsersWithoutPrivateFields()
    {
        await using var identity = new AppIdentityDbContext(new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseNpgsql(Connection).Options);
        await identity.Database.MigrateAsync();
        var manager = new AppRole { Id = 1, Name = "Manager", NormalizedName = "MANAGER" };
        var admin = new AppRole { Id = 2, Name = "Admin", NormalizedName = "ADMIN" };
        var regular = new AppRole { Id = 3, Name = "User", NormalizedName = "USER" };
        identity.Roles.AddRange(manager, admin, regular);
        foreach (var (id, name, role, locked) in new[]
        {
            (17, "Synthetic Alpha", manager, false),
            (18, "Synthetic Beta", admin, false),
            (19, "Synthetic Locked", manager, true),
            (20, "Synthetic User", regular, false)
        })
        {
            var user = new AppUser
            {
                Id = id,
                UserName = "synthetic" + id,
                NormalizedUserName = "SYNTHETIC" + id,
                FullName = name,
                LockoutEnabled = true,
                LockoutEnd = locked ? DateTimeOffset.UtcNow.AddDays(1) : null
            };
            user.UserRoles.Add(new AppUserRole { User = user, Role = role, AssignedAt = DateTime.UtcNow });
            identity.Users.Add(user);
        }
        await identity.SaveChangesAsync();
        var services = new ServiceCollection().AddLogging().AddSingleton(identity);
        services.AddIdentityCore<AppUser>().AddRoles<AppRole>().AddEntityFrameworkStores<AppIdentityDbContext>();
        await using var provider = services.BuildServiceProvider();
        var reader = new QuoteAssigneeReader(provider.GetRequiredService<UserManager<AppUser>>());
        var first = await reader.SearchAsync("Synthetic", 1, 1, default);
        var second = await reader.SearchAsync("Synthetic", 2, 1, default);
        Assert.That(first.PagedInfo.TotalRecords, Is.EqualTo(2));
        Assert.That(first.Value.Single().UserId, Is.EqualTo(17));
        Assert.That(second.Value.Single().UserId, Is.EqualTo(18));
        Assert.That(await reader.IsEligibleAsync(17, default), Is.True);
        Assert.That(await reader.IsEligibleAsync(18, default), Is.True);
        Assert.That(await reader.IsEligibleAsync(19, default), Is.False);
        Assert.That(await reader.IsEligibleAsync(20, default), Is.False);
        Assert.That(await reader.IsEligibleAsync(999, default), Is.False);
    }
}
