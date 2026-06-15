using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingSystem.Application.Features.Bookings.Commands.GuestSeated;

public class GuestSeatedHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUserService,
    ITableAvailabilityChecker tableAvailabilityChecker) : IRequestHandler<GuestSeatedCommand, Result>
{
    public async Task<Result> Handle(GuestSeatedCommand request, CancellationToken cancellationToken)
    {
        var res = await GetBookingAndRestaurantOwnerId(request.BookingId, cancellationToken);
        if (res is null or { Booking: null }) return Result.Fail(BookingErrors.NotFound);
        if (res.RestaurantOwnerId != currentUserService.UserId) return Result.Fail(BookingErrors.AccessDenied);

        if (res.Booking.CanGuestSit() is { IsFailed: true } failed) return failed;

        var now = DateTimeOffset.Now;
        IDbContextTransaction? transaction = null;

        try
        {
            switch (res.Booking.GetAvailabilityState(now))
            {
                case BookingAvailabilityState.Early:
                    transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                    if (!await tableAvailabilityChecker.IsTableAvailableAsync(res.Booking.TableId, now,
                            res.Booking.TimeSlot.Start, transaction.GetDbTransaction(), cancellationToken))
                        return Result.Fail(BookingErrors.TableNotAvailable);
                    break;
                case BookingAvailabilityState.Valid: break;
                case BookingAvailabilityState.Expired:
                    return Result.Fail(new InternalServerError("Booking.Expired",
                        "Booking time slot has already passed"));
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (res.Booking.GuestSeated() is { IsFailed: true } failed2) return failed2;
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }

        return Result.Ok();
    }

    private sealed record BookingAndRestaurantOwnerId(Booking Booking, UserId RestaurantOwnerId);

    private async Task<BookingAndRestaurantOwnerId?> GetBookingAndRestaurantOwnerId(Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings.Where(b => b.Id == new BookingId(bookingId))
            .Select(b => new BookingAndRestaurantOwnerId(
                Booking: b,
                RestaurantOwnerId: b.Table.Restaurant.OwnerId
            )).FirstOrDefaultAsync(cancellationToken);
    }
}