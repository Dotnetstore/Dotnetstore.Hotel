namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public static class IdentificationTypes
{
    public const string Passport = "Passport";

    public const string NationalId = "NationalId";

    public static readonly string[] All = [Passport, NationalId];

    public static bool IsValid(string identificationType) => All.Contains(identificationType, StringComparer.OrdinalIgnoreCase);
}
