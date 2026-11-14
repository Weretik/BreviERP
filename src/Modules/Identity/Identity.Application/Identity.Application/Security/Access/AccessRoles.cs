using Identity.Domain.Authorization;

namespace Identity.Application.Security.Access;

/// <summary>The built-in role names the Api's named policies require.</summary>
public static class AccessRoles
{
    public const string Admin = RoleNames.Admin;
    public const string Manager = RoleNames.Manager;
    public const string User = RoleNames.User;
    public const string Accountant = RoleNames.Accountant;
    public const string WorkshopChief = RoleNames.WorkshopChief;
    public const string Seo = RoleNames.Seo;
}
