using System.Text;
using BookingSystem.Domain.Users.Errors;
using FluentResults;

namespace BookingSystem.Domain.Users.ValueObjects;

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
    public static Result<byte[]> FromString(string token)
    {
        int maxDecodedLength = (token.Length * 3) / 4;
        Span<byte> buffer = stackalloc byte[maxDecodedLength];

        if (Convert.TryFromBase64String(token, buffer, out int bytesWritten))
        {
            ReadOnlySpan<byte> actualData = buffer.Slice(0, bytesWritten);
            return actualData.ToArray();
        }
        return RefreshTokenErrors.Invalid;
    }
}