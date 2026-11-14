using Identity.Api.Access;
using Identity.Application.Security.Access;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.Api.DependencyInjection;

public static class IdentityAuthorizationExtensions
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
                policy.RequireRole(AccessRoles.Admin));

            options.AddPolicy(PolicyNames.RequireManagerRole, policy =>
                policy.RequireRole(AccessRoles.Manager, AccessRoles.Admin));

            options.AddPolicy(PolicyNames.CanManageUsers, policy =>
                policy.RequireRole(AccessRoles.Admin));

            options.AddPolicy(PolicyNames.CanManageOrders, policy =>
                policy.RequireRole(AccessRoles.Manager, AccessRoles.Admin));

            // Shared catalogue reads (pickers in КП, workshop and reference pages): the director also holds Admin; a role
            // made on «Права доступу» reads too, its pages decide what it changes.
            options.AddPolicy(PolicyNames.CatalogRead, policy =>
                policy.RequireAssertion(context => AccessCheck.HasAnyRole(context.User,
                    AccessRoles.User, AccessRoles.Accountant, AccessRoles.Manager, AccessRoles.Seo, AccessRoles.Admin)));

            // «Мій кабінет»: every staff account, including roles made on «Права доступу»; the director also holds Admin.
            options.AddPolicy(PolicyNames.OwnProfile, policy =>
                policy.RequireAssertion(context => AccessCheck.HasAnyRole(context.User,
                    AccessRoles.Accountant, AccessRoles.Manager, AccessRoles.WorkshopChief, AccessRoles.Seo, AccessRoles.Admin)));

            options.AddPolicy(PolicyNames.OrderCreate, policy =>
                policy.RequireRole(AccessRoles.User, AccessRoles.Manager, AccessRoles.Admin));
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
}
