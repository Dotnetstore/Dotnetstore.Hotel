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
}
