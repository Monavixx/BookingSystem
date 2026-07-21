using System.Data;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.Options;
using BookingSystem.Application.Features.Bookings.Abstractions;
using BookingSystem.Application.Features.Bookings.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.Services;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Bookings.ValueObjects.Helpers;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace BookingSystem.Application.Features.Bookings.Commands.Create;

public class CreateBookingHandler(AppDbContext dbContext, BookingDurationCalculator durationCalculator,
    IBackgroundJobService backgroundJobService, IOptions<BookingOptions> bookingOptions,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private sealed record TableDto(int Capacity, Guid RestaurantId, int TableNumber);

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        var durationResult = durationCalculator.CalculateDuration(request.GuestCount);
        if (durationResult.IsFailed) return durationResult.ToResult<BookingDto>();

        var scheduledAt = request.ScheduledAt.ToUniversalTime();
        var endTime = scheduledAt.Add(durationResult.Value);

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        Booking? booking;
        try
        {
            var dbTransaction = transaction.GetDbTransaction();

            var table = await GetTableAsync(dbTransaction, request.RestaurantId, request.GuestCount, scheduledAt,
                request.TableNumber, endTime);
            if (table is null) return TableErrors.NotFound;
            if (table.Capacity < request.GuestCount)
                return BookingErrors.CapacityExceeded;

            var slot = BookingTimeSlot.Create(scheduledAt, endTime);
            if (slot.IsFailed) return slot.ToResult<BookingDto>();

            var bookingRes = Booking.Create(currentUserService.GetRequiredUserId(), request.GuestCount,
                new RestaurantId(request.RestaurantId), table.TableNumber, slot.Value);
            if (bookingRes.IsFailed) return bookingRes.ToResult<BookingDto>();
            booking = bookingRes.Value;
            
            dbContext.Bookings.Add(booking);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        backgroundJobService.Schedule<IBookingCancellationService>(
            s => s.CancelAsync(booking.Id, CancellationReason.PendingTimeout),
            TimeSpan.FromMinutes(bookingOptions.Value.GuestConfirmationTimeoutMinutes));
        
        return new BookingDto(booking);
    }

    private async Task<TableDto?> GetTableAsync(IDbTransaction transaction, Guid restaurantId, int guestCount,
        DateTimeOffset scheduledAt, int? tableNumber, DateTimeOffset endTime)
    {
        if (tableNumber is null)
        {
            return await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<TableDto>(
                """
                 SELECT t.capacity as Capacity, 
                        t.restaurant_id as RestaurantId, 
                        t.table_number as TableNumber
                 FROM tables t
                 WHERE t.restaurant_id = @RestaurantId 
                   AND t.capacity >= @GuestCount 
                   AND NOT EXISTS (
                       SELECT 1 FROM bookings b
                       WHERE b.restaurant_id = t.restaurant_id
                         AND b.table_number = t.table_number
                         AND b.start_time < @EndTime
                         AND b.end_time > @ScheduledAt 
                         AND b.status != ALL(@FinalStatuses)
                   )
                 ORDER BY t.capacity
                 LIMIT 1
                 """,
                new
                {
                    RestaurantId = restaurantId, GuestCount = guestCount, ScheduledAt = scheduledAt, EndTime = endTime,
                    FinalStatuses = BookingStatusHelper.FinalIntStatuses
                }, transaction: transaction);
        }

        return await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<TableDto>(
            """
            SELECT capacity as Capacity, 
                   restaurant_id as RestaurantId, 
                   table_number as TableNumber
            FROM tables
            WHERE restaurant_id = @RestaurantId AND table_number = @TableNumber
            LIMIT 1
            """,
            new { RestaurantId = restaurantId, TableNumber = tableNumber }, transaction: transaction);
    }
}