using Identity.Application.Security.Policies;
using Identity.Api.Endpoints.Users.ChangeStaffRole;
using Identity.Api.Endpoints.Users.CreateStaffAccount;
using Identity.Api.Endpoints.Users.DeleteStaffAccount;
using Identity.Api.Endpoints.Users.GetStaffAccounts;
using Identity.Api.Endpoints.Users.SetStaffBlocked;
using Identity.Api.Endpoints.Users.SetStaffPassword;
using Identity.Api.Endpoints.Users.UpdateStaffDetails;

namespace Identity.Api.Endpoints.Users;

/// <summary>Staff accounts (director, administrators, accountants, managers), managed by an administrator.
/// Only the director changes the director (403 otherwise); there is at most one director (409).</summary>
internal static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/users").RequireAuthorization(PolicyNames.CanManageUsers).WithTags("AdminUsers");
        GetStaffAccountsEndpoint.Map(group);
        CreateStaffAccountEndpoint.Map(group);
        ChangeStaffRoleEndpoint.Map(group);
        SetStaffBlockedEndpoint.Map(group);
        UpdateStaffDetailsEndpoint.Map(group);
        SetStaffPasswordEndpoint.Map(group);
        DeleteStaffAccountEndpoint.Map(group);
        return app;
    }
}
