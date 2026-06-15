using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Create;

public sealed record CreateBookingCommand(
    Guid GuestId,
    int GuestCount,
    Guid RestaurantId,
    int? TableNumber,
    DateTimeOffset ScheduledAt)
    : IRequest<Result<CreateBookingResponse>>;