namespace Identity.Domain.Authorization;

public static class RoleNames
{
    public const string Director = "Director";
    public const string Admin = "Admin";
    public const string Accountant = "Accountant";
    public const string User = "User";
    public const string Manager = "Manager";

    /// <summary>«Начальник цеху»: runs the sewing workshop («Швейний цех»).</summary>
    public const string WorkshopChief = "WorkshopChief";

    /// <summary>«SEO-фахівець»: the site texts and search settings («Контент»).</summary>
    public const string Seo = "Seo";
    public const string Guest = "Guest";

    public static readonly IReadOnlyList<string> All =
    [
        Director,
        Admin,
        Accountant,
        Manager,
        WorkshopChief,
        Seo,
        User,
        Guest
    ];
}
