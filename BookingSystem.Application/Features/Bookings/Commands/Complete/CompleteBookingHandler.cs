using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Bookings.Commands.Complete;

public sealed class CompleteBookingHandler(AppDbContext dbContext, ICurrentUserService currentUserService,
    IBookingCompletionService bookingCompletionService)
    : IRequestHandler<CompleteBookingCommand, Result>
{
    public async Task<Result> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
    {
        var res = await dbContext.Bookings.Where(b => b.Id == new BookingId(request.BookingId))
            .Select(b => new
            {
                Booking = b,
                RestaurantOwnerId = b.Table.Restaurant.OwnerId
            }).FirstOrDefaultAsync(cancellationToken);
        if (res is null) return Result.Fail(BookingErrors.NotFound);

        if (res.RestaurantOwnerId != currentUserService.UserId) return Result.Fail(BookingErrors.AccessDenied);
        var booking = res.Booking;

        return await bookingCompletionService.Complete(booking, cancellationToken);
    }
}
