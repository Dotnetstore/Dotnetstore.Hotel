namespace Dotnetstore.Hotel.Api.Users.Domain;

public static class Roles
{
    public const string Administrator = "administrator";
    public const string Superuser = "superuser";
    public const string Desk = "desk";
    public const string Restaurant = "restaurant";
    public const string Cleaning = "cleaning";
    public const string Maintenance = "maintenance";

    public static readonly string[] All =
    [
        Administrator, Superuser, Desk, Restaurant, Cleaning, Maintenance,
    ];

    /// <summary>
    /// Administrator/superuser are hardcoded in the "AdminOnly" authorization policy (Program.cs) by name,
    /// so renaming or deleting either would lock everyone out of every admin feature with no recovery path.
    /// </summary>
    public static bool IsProtected(string roleName)
        => string.Equals(roleName, Administrator, StringComparison.OrdinalIgnoreCase)
           || string.Equals(roleName, Superuser, StringComparison.OrdinalIgnoreCase);
}
