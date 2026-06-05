using System.Data;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.Services;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users.ValueObjects;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingSystem.Application.Features.Bookings.Commands.CreateBooking;

public class CreateBookingHandler(AppDbContext dbContext, BookingDurationCalculator durationCalculator) : IRequestHandler<CreateBookingCommand, Result<CreateBookingResponse>>
{
    private sealed record TableDto(int Capacity, Guid RestaurantId, int TableNumber);
    public async Task<Result<CreateBookingResponse>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var durationResult = durationCalculator.CalculateDuration(request.GuestCount);
        if(durationResult.IsFailed) return durationResult.ToResult<CreateBookingResponse>();
        
        var endTime = request.ScheduledAt.Add(durationResult.Value);
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var dbTransaction = transaction.GetDbTransaction();

            var table = await GetTableAsync(dbTransaction, request, endTime);
            if (table is null) return Result.Fail<CreateBookingResponse>(TableErrors.NotFound);
            if (table.Capacity < request.GuestCount)
                return Result.Fail<CreateBookingResponse>(BookingErrors.CapacityExceeded);

            var slot = BookingTimeSlot.Create(request.ScheduledAt, endTime);
            if (slot.IsFailed) return slot.ToResult<CreateBookingResponse>();

            var booking = Booking.Create(new UserId(request.GuestId), request.GuestCount,
                new RestaurantId(request.RestaurantId), table.TableNumber, slot.Value);
            if (booking.IsFailed) return booking.ToResult<CreateBookingResponse>();

            dbContext.Bookings.Add(booking.Value);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return new CreateBookingResponse(booking.Value.Id.Value, booking.Value.TimeSlot.Start,
                booking.Value.TimeSlot.End, table.TableNumber);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<TableDto?> GetTableAsync(IDbTransaction transaction, CreateBookingCommand request, DateTimeOffset endTime)
    {
        if (request.TableNumber is null)
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
                  )
                ORDER BY t.capacity
                LIMIT 1
                FOR UPDATE
                """, new { request.RestaurantId, request.GuestCount, request.ScheduledAt, EndTime = endTime }, transaction: transaction);
        }
        return await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<TableDto>(
            """
            SELECT capacity as Capacity, 
                   restaurant_id as RestaurantId, 
                   table_number as TableNumber
            FROM tables
            WHERE restaurant_id = @RestaurantId AND table_number = @TableNumber
            LIMIT 1
            FOR UPDATE
            """,
            new { request.RestaurantId, request.TableNumber }, transaction: transaction);
    }
}