using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Bookings.Commands.ConfirmBookingByGuest;

public class ConfirmBookingByGuestHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUserService,
    ILogger<ConfirmBookingByGuestHandler> logger)
    : IRequestHandler<ConfirmBookingByGuestCommand, Result>
{
    public async Task<Result> Handle(ConfirmBookingByGuestCommand request, CancellationToken cancellationToken)
    {
        var booking = await dbContext.Bookings.FindAsync([new BookingId(request.BookingId)], cancellationToken);
        if (booking is null) return Result.Fail(BookingErrors.NotFound);

        if (currentUserService.UserId != booking.GuestId)
            return Result.Fail(BookingErrors.AccessDenied);
        if (booking.ConfirmByGuest() is { IsFailed: true } failed)
            return failed;
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Booking {BookingId} confirmed by guest", request.BookingId);
        return Result.Ok();
    }
}