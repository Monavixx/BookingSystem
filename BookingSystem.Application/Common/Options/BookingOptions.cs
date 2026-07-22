using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace BookingSystem.Application.Common.Options;

[UsedImplicitly (ImplicitUseTargetFlags.WithMembers)]
public class BookingOptions
{
    public const string SectionName = "Booking";
    
    [Required] public int GuestConfirmationTimeoutMinutes { get; init; }
    [Required] public int MaxBookingCancellation { get; init; }
    [Required] public TimeSpan BookingCancellationPeriod { get; init; }
    [Required] public TimeSpan ViolationCancellationPolicyBlockDuration { get; init; }
}