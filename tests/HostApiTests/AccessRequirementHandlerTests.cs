using System.Security.Claims;
using Host.Api.Security.Access;
using Identity.Application.Security.Access;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HostApiTests;

/// <summary>The «Права доступу» check: the director always passes; others need «Перегляд» and the asked action.</summary>
public sealed class AccessRequirementHandlerTests
{
    [TestCase("Director", "quotes", "DELETE", true)]
    [TestCase("Manager", "quotes", "GET", true)]
    [TestCase("Manager", "quotes", "POST", true)]
    [TestCase("Manager", "quotes", "DELETE", false)]
    [TestCase("Manager", "payroll", "GET", false)]
    [TestCase("WorkshopChief", "fabric-stock", "POST", true)]
    [TestCase("Accountant", "products", "GET", true)]
    [TestCase("Accountant", "products", "PUT", false)]
    [TestCase("custom-1a2b3c", "products", "GET", false)]
    public async Task Default_rights_decide(string role, string area, string method, bool allowed)
    {
        Assert.That(await CheckAsync(new DefaultAccessGrantSource(), role, area, method), Is.EqualTo(allowed));
    }

    [Test]
    public async Task Saved_rights_replace_the_defaults_and_need_view()
    {
        var saved = new FixedGrants(new Dictionary<string, AccessAction>
        {
            [AccessCatalog.Products] = AccessAction.View | AccessAction.Delete,
            [AccessCatalog.Media] = AccessAction.Edit
        });

        Assert.Multiple(async () =>
        {
            Assert.That(await CheckAsync(saved, "custom-1a2b3c", "products", "DELETE"), Is.True);
            Assert.That(await CheckAsync(saved, "custom-1a2b3c", "products", "PUT"), Is.False);
            Assert.That(await CheckAsync(saved, "custom-1a2b3c", "media", "PUT"), Is.False);
        });
    }

    private static async Task<bool> CheckAsync(IAccessGrantSource source, string role, string area, string method)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = method;
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        var requirement = new AccessRequirement(AccessCatalog.Find(area)!, null);
        var context = new AuthorizationHandlerContext([requirement], user, http);
        await new AccessRequirementHandler(source, new HttpContextAccessor()).HandleAsync(context);
        return context.HasSucceeded;
    }

    private sealed class FixedGrants(IReadOnlyDictionary<string, AccessAction> grants) : IAccessGrantSource
    {
        public Task<IReadOnlyDictionary<string, AccessAction>> GrantsOfAsync(
            IReadOnlyCollection<string> roles,
            CancellationToken cancellationToken = default) => Task.FromResult(grants);
    }
}
