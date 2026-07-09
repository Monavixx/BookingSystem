using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class BookingCancellationService(
    AppDbContext dbContext,
    TimeProvider timeProvider,
    ICurrentUserService currentUserService,
    IBackgroundJobService backgroundJobService) : IBookingCancellationService
{
    public async Task<Result<bool>> CancelAsync(BookingId bookingId, CancellationReason reason)
    {
        var booking = await dbContext.Bookings.FindAsync(bookingId);
        return await CancelAsync(booking, reason);
    }

    public async Task<Result<bool>> CancelAsync(Booking? booking, CancellationReason reason)
    {
        if (booking is null) return BookingErrors.NotFound;

        dbContext.Bookings.Entry(booking).State = EntityState.Modified;

        var res = booking.Cancel(reason);
        if (res.IsFailed) return res;

        if (res.Value)
            dbContext.CancellationRecords.Add(
                CancellationRecord.Create(timeProvider, currentUserService.UserId, booking.Id, reason));
        await dbContext.SaveChangesAsync();
        
        if (res.Value)
            backgroundJobService.Enqueue<IUserBlocker>
                (u => u.BlockUserIfCancellationPolicyViolated(booking.GuestId));
        
        return res;
    }
}