using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Infrastructure.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Secret { get; init; } = null!;
    [Required] public int ExpirationMinutes { get; init; }
    [Required] public string Issuer { get; init; } = null!;
    [Required] public string Audience { get; init; } = null!;
}