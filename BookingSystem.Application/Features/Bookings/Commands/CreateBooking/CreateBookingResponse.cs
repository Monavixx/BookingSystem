namespace BookingSystem.Application.Features.Bookings.Commands.CreateBooking;

public sealed record CreateBookingResponse(Guid BookingId, DateTimeOffset StartTime, DateTimeOffset EndTime, int TableNumber);