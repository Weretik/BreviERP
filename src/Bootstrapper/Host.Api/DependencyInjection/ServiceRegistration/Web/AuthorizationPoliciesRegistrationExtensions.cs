using Host.Api.Security.Access;
using Identity.Application.Security.Access;
using Identity.Application.Security.Policies;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class AuthorizationPoliciesRegistrationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAccessPolicies();
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(PolicyNames.RequireAdminRole, policy =>
                policy.RequireRole(RoleNames.Admin));

            options.AddPolicy(PolicyNames.RequireManagerRole, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageUsers, policy =>
                policy.RequireRole(RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageOrders, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));

            // Shared catalogue reads (pickers in КП, workshop and reference pages): the director also holds Admin; a role
            // made on «Права доступу» reads too, its pages decide what it changes.
            options.AddPolicy(PolicyNames.CatalogRead, policy =>
                policy.RequireAssertion(context => HasAnyRole(context.User,
                    RoleNames.User, RoleNames.Accountant, RoleNames.Manager, RoleNames.Admin)));

            // «Мій кабінет»: every staff account, including roles made on «Права доступу»; the director also holds Admin.
            options.AddPolicy(PolicyNames.OwnProfile, policy =>
                policy.RequireAssertion(context => HasAnyRole(context.User,
                    RoleNames.Accountant, RoleNames.Manager, RoleNames.WorkshopChief, RoleNames.Admin)));

            options.AddPolicy(PolicyNames.OrderCreate, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin));
        });

        return services;
    }

    /// <summary>
    /// «Права доступу»: work pages use <c>access:&lt;page&gt;[:&lt;action&gt;]</c> policies, checked against the rights saved
    /// for each role (Identity registers the database source; without it the catalogue defaults apply).
    /// </summary>
    public static IServiceCollection AddAccessPolicies(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationPolicyProvider, AccessPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, AccessRequirementHandler>();
        services.TryAddScoped<IAccessGrantSource, DefaultAccessGrantSource>();
        return services;
    }

    /// <summary>One of the given built-in roles, or a role made on «Права доступу».</summary>
    private static bool HasAnyRole(System.Security.Claims.ClaimsPrincipal user, params string[] builtIn) =>
        AccessClaims.RolesOf(user).Any(role =>
            builtIn.Contains(role, StringComparer.Ordinal) || (!AccessCatalog.IsBuiltIn(role) && !AccessCatalog.IsOutsider(role)));
}
