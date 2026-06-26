namespace Dotnetstore.Hotel.Api.Users.Domain;

public class RefreshToken
{
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    private RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive => RevokedAtUtc is null && DateTimeOffset.UtcNow < ExpiresAtUtc;

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
        => new(userId, tokenHash, expiresAtUtc);

    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
