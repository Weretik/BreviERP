using Identity.Application.Security.Policies;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class AuthorizationPoliciesRegistrationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
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

            options.AddPolicy(PolicyNames.CanManageProducts, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));

            options.AddPolicy(PolicyNames.CanManageOrders, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.Admin));

            // The director also holds Admin; the accountant only reads the catalogue for now.
            options.AddPolicy(PolicyNames.CatalogRead, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Accountant, RoleNames.Manager, RoleNames.Admin));

            // «Мій кабінет»: every staff account; the director also holds Admin.
            options.AddPolicy(PolicyNames.OwnProfile, policy =>
                policy.RequireRole(RoleNames.Accountant, RoleNames.Manager, RoleNames.WorkshopChief, RoleNames.Admin));

            // «Швейний цех»: the head of the workshop; the director also holds Admin.
            options.AddPolicy(PolicyNames.CanManageWorkshop, policy =>
                policy.RequireRole(RoleNames.WorkshopChief, RoleNames.Admin));

            // «Виробничі завдання»: managers create them from their КП, the head of the workshop sews them.
            options.AddPolicy(PolicyNames.CanUseProductionOrders, policy =>
                policy.RequireRole(RoleNames.Manager, RoleNames.WorkshopChief, RoleNames.Admin));

            // «Рахунки на закупку»: the accountant and Admin; the director also holds Admin. Managers do not see it.
            options.AddPolicy(PolicyNames.CanUsePurchaseInvoices, policy =>
                policy.RequireRole(RoleNames.Accountant, RoleNames.Admin));

            // «Бухгалтерія»: only the Director (user decision 2026-11-05); Admin and Accountant do not see it.
            options.AddPolicy(PolicyNames.CanUseAccounting, policy =>
                policy.RequireRole(RoleNames.Director));

            options.AddPolicy(PolicyNames.OrderCreate, policy =>
                policy.RequireRole(RoleNames.User, RoleNames.Manager, RoleNames.Admin));
        });

        return services;
    }
}
