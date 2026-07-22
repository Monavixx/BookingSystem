using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace BookingSystem.Infrastructure.Options;

[UsedImplicitly (ImplicitUseTargetFlags.WithMembers)]
public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    [Required] public int ExpirationDays { get; init; }
}