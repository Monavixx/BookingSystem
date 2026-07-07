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

public class CancelBookingHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IBackgroundJobService backgroundJobService) : IRequestHandler<CancelBookingCommand, Result>
{
    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var bookingInfo = await dbContext.Bookings
            .Where(b => b.Id == new BookingId(request.BookingId))
            .Select(b => new { Booking = b, RestaurantOwnerId = b.Table.Restaurant.OwnerId, BookingGuestRole = b.Guest.Role })
            .FirstOrDefaultAsync(cancellationToken);
        if (bookingInfo is null)
        {
            return Result.Fail(BookingErrors.NotFound);
        }

        var booking = bookingInfo.Booking;
        var guestRole = bookingInfo.BookingGuestRole;

        var curUser = (await currentUserService.GetUserAsync())!;
        if (!CanAccess(booking, bookingInfo.RestaurantOwnerId, curUser))
            return Result.Fail(BookingErrors.AccessDenied);

        if (booking.Cancel() is { IsFailed: true } cancelError)
            return cancelError;

        bool shouldCreateCancellationRecord = guestRole is UserRole.Guest &&
                                              (curUser.Role is not (UserRole.Manager or UserRole.Admin) ||
                                               request.IsGuestRequest);
        if (shouldCreateCancellationRecord)
        {
            var cr = CancellationRecord.Create(timeProvider, currentUserService.UserId, booking.Id);
            dbContext.CancellationRecords.Add(cr);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        
        if (shouldCreateCancellationRecord)
        {
            backgroundJobService.Enqueue<IUserBlocker>
                (u => u.BlockUserIfCancellationPolicyViolated(booking.GuestId));
        }

        return Result.Ok();
    }

    private static bool CanAccess(Booking booking, UserId restaurantOwnerId, User? curUser)
    {
        if (curUser is null) return false;
        return curUser.Role is UserRole.Admin ||
               (curUser.Role is UserRole.Manager &&
                (curUser.Id == restaurantOwnerId || curUser.Id == booking.GuestId)) ||
               (curUser.Role is UserRole.Guest && curUser.Id == booking.GuestId);
    }
}