using Dotnetstore.Hotel.Api.Users.Authentication;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class RefreshTokenGeneratorTests
{
    [Fact]
    public void Generate_TwoCalls_ProduceDifferentRawTokens()
    {
        var (firstRaw, _) = RefreshTokenGenerator.Generate();
        var (secondRaw, _) = RefreshTokenGenerator.Generate();

        firstRaw.ShouldNotBe(secondRaw);
    }

    [Fact]
    public void Generate_ReturnsHashMatchingRawToken()
    {
        var (raw, hash) = RefreshTokenGenerator.Generate();

        RefreshTokenGenerator.Hash(raw).ShouldBe(hash);
    }

    [Fact]
    public void Hash_DifferentInputs_ProduceDifferentHashes()
    {
        RefreshTokenGenerator.Hash("token-a").ShouldNotBe(RefreshTokenGenerator.Hash("token-b"));
    }
}
