using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Application.Common.Options;

public class BookingOptions
{
    public const string SectionName = "Booking";
    
    [Required] public int GuestConfirmationTimeoutMinutes { get; init; }
}