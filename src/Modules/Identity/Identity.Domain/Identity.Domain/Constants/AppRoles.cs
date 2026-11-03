namespace Identity.Domain.Constants;
public static class AppRoles
{
    public const string Director = "Director";
    public const string Admin = "Admin";
    public const string Accountant = "Accountant";
    public const string User = "User";
    public const string Manager = "Manager";
    public const string Guest = "Guest";

    public static readonly IReadOnlyList<string> All =
    [
        Director,
        Admin,
        Accountant,
        Manager,
        User,
        Guest
    ];
}

