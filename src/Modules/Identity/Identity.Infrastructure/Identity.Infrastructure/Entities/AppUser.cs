namespace Identity.Infrastructure.Entities;

public class AppUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Avatar file key relative to the avatars folder; the URL is built at read time.</summary>
    public string? AvatarPath { get; set; }
    public virtual ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
