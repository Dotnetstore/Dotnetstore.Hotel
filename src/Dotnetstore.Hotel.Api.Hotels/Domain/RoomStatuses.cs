namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public static class RoomStatuses
{
    public const string Available = "Available";

    public const string Maintenance = "Maintenance";

    public const string OutOfService = "OutOfService";

    public static readonly string[] All = [Available, Maintenance, OutOfService];

    public static bool IsValid(string status) => All.Contains(status, StringComparer.OrdinalIgnoreCase);
}
