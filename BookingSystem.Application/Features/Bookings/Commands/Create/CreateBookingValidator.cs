using FluentValidation;

namespace BookingSystem.Application.Features.Bookings.Commands.Create;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x=>x.ScheduledAt)
            .Must(x => x > DateTimeOffset.UtcNow)
            .WithMessage("Scheduled time must be in the future");
        RuleFor(x=>x.GuestCount)
            .GreaterThan(0)
            .WithMessage("Guest count must be a positive number");
    }
}