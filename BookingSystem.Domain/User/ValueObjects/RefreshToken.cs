namespace BookingSystem.Domain.User.ValueObjects;

public record RefreshToken
{
    public const int TokenLength = 32;
    
    private RefreshToken() { }

    public byte[] Token { get; private init; } = null!;
    public DateTime ExpiresAt { get; private init; }

    public static RefreshToken Create(byte[] token, DateTime expiresAt)
        => new()
        {
            Token = token,
            ExpiresAt = expiresAt
        };

    public override string ToString()
    {
        return Convert.ToBase64String(Token);
    }
    public static byte[] FromString(string token)
    {
        return Convert.FromBase64String(token);
    }
}