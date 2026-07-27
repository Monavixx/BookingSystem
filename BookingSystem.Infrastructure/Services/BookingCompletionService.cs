using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Services;

public class BookingCompletionService (AppDbContext dbContext) : IBookingCompletionService
{
    public Task<Result> Complete(Booking booking, CancellationToken cancellationToken)
    {
        dbContext.Entry(booking).State = EntityState.Modified;
        return CompleteInner(booking, cancellationToken);
    }

    public async Task<Result> Complete(BookingId bookingId, CancellationToken cancellationToken)
    {
        var booking = await dbContext.Bookings.FindAsync([bookingId], cancellationToken);
        if (booking is null) return BookingErrors.NotFound;
        
        return await CompleteInner(booking, cancellationToken);
    }
    
    private async Task<Result> CompleteInner(Booking booking, CancellationToken cancellationToken)
    {
        var res = booking.Complete();
        if (res.IsFailed) return res;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}