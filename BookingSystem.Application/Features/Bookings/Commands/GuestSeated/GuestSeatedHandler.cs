using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.Commands.Complete.CompleteBySystem;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Bookings.ValueObjects.Helpers;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Bookings.Commands.GuestSeated;

public class GuestSeatedHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUserService,
    ILogger<GuestSeatedHandler> logger,
    IBackgroundJobService backgroundJobService,
    TimeProvider timeProvider) : IRequestHandler<GuestSeatedCommand, Result>
{
    public async Task<Result> Handle(GuestSeatedCommand request, CancellationToken cancellationToken)
    {
        if(await currentUserService.GetUserAsync() is {Role: not UserRole.Manager})
            return Result.Fail(BookingErrors.AccessDenied);
        
        var res = await GetBookingAndRestaurantOwnerId(request.BookingId, cancellationToken);
        if (res is null) return Result.Fail(BookingErrors.NotFound);
        var booking = res.Booking;
        logger.LogDebug(
            "Successfully retrieved booking with id {BookingId} and its restaurant owner id {RestaurantOwnerId}",
            request.BookingId, res.RestaurantOwnerId);
        if (res.RestaurantOwnerId != currentUserService.UserId) return Result.Fail(BookingErrors.AccessDenied);
        logger.LogDebug("Successful authorization");

        if (booking.CanGuestSit() is { IsFailed: true } failed) return failed;
        logger.LogDebug("Domain logic checks passed");

        var now = timeProvider.GetUtcNow();
        var availabilityState = booking.GetAvailabilityState(now);
        if (availabilityState is BookingAvailabilityState.Expired)
        {
            logger.LogError(
                "The booking is expired but has the Confirmed status, which should not happen. Booking id: {BookingId}",
                request.BookingId);
            return Result.Fail(BookingErrors.Expired);
        }

        if (booking.GuestSeated() is { IsFailed: true } failed2) return failed2;

        if (availabilityState is BookingAvailabilityState.Valid)
        {
            var rows = await dbContext.Database.ExecuteSqlRawAsync(
                """
                UPDATE Bookings b
                SET status = {0}
                WHERE b.id = {1} AND b.xmin = {2}::text::xid
                """,
                BookingStatus.Seated, booking.Id.Value, booking.RowVersion);
            if (rows == 0)
            {
                return BookingErrors.HasBeenChanged;
            }
        }
        else if (availabilityState is BookingAvailabilityState.Early)
        {
            var rows = await dbContext.Database.ExecuteSqlRawAsync(
                """
                UPDATE Bookings b
                SET status = {0}, start_time = {5}, end_time={6}
                WHERE b.id = {1} AND b.xmin = {2}::text::xid
                  AND NOT EXISTS (
                      SELECT 1 FROM Bookings b2
                      WHERE b2.id != {1}
                        AND b2.restaurant_id = {3}
                        AND b2.table_number = {4}
                        AND b2.start_time < {6}
                        AND b2.end_time > {5}
                        AND b2.status != ALL({7})
                  )
                """,
                    booking.Status, booking.Id.Value, booking.RowVersion,
                    booking.RestaurantId.Value, booking.TableNumber,
                    now, now + (booking.TimeSlot.End-booking.TimeSlot.Start),
                    BookingStatusHelper.FinalIntStatuses
                );
            if (rows == 0)
            {
                return await DiagnoseError(booking.Id, booking.RowVersion);
            }
        }

        logger.LogInformation("Successfully updated booking");

        backgroundJobService.Schedule<IMediator>(
            // ReSharper disable once MethodSupportsCancellation
            s => s.Send(new CompleteBookingBySystemCommand(booking.Id.Value)),
            booking.TimeSlot.End);
        return Result.Ok();
    }

    private sealed record BookingAndRestaurantOwnerId(Booking Booking, UserId RestaurantOwnerId);

    private async Task<BookingAndRestaurantOwnerId?> GetBookingAndRestaurantOwnerId(Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Bookings.AsNoTracking().Where(b => b.Id == new BookingId(bookingId))
            .Select(b => new BookingAndRestaurantOwnerId(
                Booking: b,
                RestaurantOwnerId: b.Table.Restaurant.OwnerId
            )).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Result> DiagnoseError(BookingId id, uint rowVersion)
    {
        var res = (await dbContext.Database.SqlQueryRaw<int>(
            """
            SELECT COUNT(*) AS Value FROM (
                WITH TheBooking AS (SELECT b.xmin FROM Bookings b WHERE b.id = {0})
                SELECT 1 FROM TheBooking
                UNION ALL
                SELECT 1 FROM TheBooking tb WHERE tb.xmin = {1}::text::xid
            ) as alias
            """, id.Value, rowVersion).ToListAsync()).Single();
        return res switch
        {
            0 => BookingErrors.NotFound,
            1 => BookingErrors.HasBeenChanged,
            2 => BookingErrors.TableNotAvailable,
            _ => new InternalServerError("Booking.DiagnoseError", "Something went wrong while diagnosing the error")
        };
    }
}