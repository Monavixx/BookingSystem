namespace BookingSystem.Application.Features.Bookings.Commands.Create;

public sealed record CreateBookingResponse(Guid BookingId, DateTimeOffset StartTime, DateTimeOffset EndTime, int TableNumber);