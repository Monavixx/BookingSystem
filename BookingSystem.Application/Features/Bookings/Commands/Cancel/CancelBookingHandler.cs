using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Bookings.Commands.Cancel;

public class CancelBookingHandler (AppDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<CancelBookingCommand, Result>
{
    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var bookingInfo = await dbContext.Bookings
            .Where(b => b.Id == new BookingId(request.BookingId))
            .Select(b => new { Booking = b, RestaurantOwnerId = b.Table.Restaurant.OwnerId })
            .FirstOrDefaultAsync(cancellationToken);
        if (bookingInfo is null)
        {
            return Result.Fail(BookingErrors.NotFound);
        }

        var booking = bookingInfo.Booking;
        
        if(!await CanAccess(booking, bookingInfo.RestaurantOwnerId))
            return Result.Fail(BookingErrors.AccessDenied);

        if (booking.Cancel() is { IsFailed: true } cancelError)
            return cancelError;
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private async ValueTask<bool> CanAccess(Booking booking, UserId restaurantOwnerId)
    {
        var user = await currentUserService.GetUserAsync();
        if (user is null) return false;

        return user.Role is UserRole.Admin ||
               (user.Role is UserRole.Manager && user.Id == restaurantOwnerId) ||
               (user.Role is UserRole.Guest && user.Id == booking.GuestId);
    }
}