using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Infrastructure.Options;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    [Required] public int ExpirationDays { get; init; }
}