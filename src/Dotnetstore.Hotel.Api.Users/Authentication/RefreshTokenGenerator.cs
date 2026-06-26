using System.Security.Cryptography;
using System.Text;

namespace Dotnetstore.Hotel.Api.Users.Authentication;

public static class RefreshTokenGenerator
{
    public static (string RawToken, string TokenHash) Generate()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (rawToken, Hash(rawToken));
    }

    public static string Hash(string rawToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
