using Identity.Application.Security.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Identity.Api.Access;

/// <summary>
/// Builds <c>access:page[:action]</c> policies on demand; every other name goes to the policies registered in
/// <c>AddAuthorizationPolicies</c>. An <c>access:</c> name for a page the catalogue lacks stays unknown and fails loudly.
/// </summary>
public sealed class AccessPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var parsed = AccessPolicies.Parse(policyName);
        if (parsed is null)
        {
            return await base.GetPolicyAsync(policyName);
        }

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new AccessRequirement(parsed.Value.Area, parsed.Value.Action))
            .Build();
    }
}
