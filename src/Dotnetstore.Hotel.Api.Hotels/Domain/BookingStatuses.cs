namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public static class BookingStatuses
{
    public const string Reserved = "Reserved";

    public const string CheckedIn = "CheckedIn";

    public const string CheckedOut = "CheckedOut";

    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Reserved, CheckedIn, CheckedOut, Cancelled];

    public static readonly string[] Active = [Reserved, CheckedIn];

    public static bool IsValid(string status) => All.Contains(status, StringComparer.OrdinalIgnoreCase);
}
