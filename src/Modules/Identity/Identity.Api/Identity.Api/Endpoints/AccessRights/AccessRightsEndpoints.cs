using Identity.Application.Security.Policies;

namespace Identity.Api.Endpoints.AccessRights;

/// <summary>«Адміністрування → Права доступу»: which pages each role sees and what it may do there, plus new roles.
/// The director always has everything (400 on save); built-in roles are never deleted (400); a role with users is not
/// deleted (409); a save over someone else's newer save is refused (409).</summary>
internal static class AccessRightsEndpoints
{
    public static IEndpointRouteBuilder MapAccessRightsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/access").RequireAuthorization(PolicyNames.CanManageUsers).WithTags("AccessRights");
        GetAccessRightsEndpoint.Map(group);
        SaveRoleAccessEndpoint.Map(group);
        CreateRoleEndpoint.Map(group);
        DeleteRoleEndpoint.Map(group);
        return app;
    }
}
