using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.Errors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Complete.CompleteBySystem;

public class CompleteBookingBySystemHandler(AppDbContext dbContext) : IRequestHandler<CompleteBookingBySystemCommand, Result>
{
    public async Task<Result> Handle(CompleteBookingBySystemCommand request, CancellationToken cancellationToken)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], cancellationToken);
        if (booking is null) return Result.Fail(BookingErrors.NotFound);

        if (booking.Complete() is { IsFailed: true } failed) return failed;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}