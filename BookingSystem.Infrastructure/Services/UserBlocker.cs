using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.Options;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using Dapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookingSystem.Infrastructure.Services;

public class UserBlocker(AppDbContext dbContext, IOptions<BookingOptions> bookingOptions, TimeProvider timeProvider) : IUserBlocker
{
    public async Task<Result> BlockUserIfCancellationPolicyViolated(UserId userId)
    {
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null) return UserErrors.NotFound;
        
        var connection = dbContext.Database.GetDbConnection();
        var shouldBlock = await connection.ExecuteScalarAsync<bool>(
            """
                SELECT count(*) >= @MaxBookingCancellation FROM (
                    SELECT 1 FROM cancellation_records cr
                    JOIN bookings b ON cr.booking_id = b.id
                    WHERE b.guest_id = @UserId 
                      AND cr.canceled_at >= NOW() - @BookingCancellationPeriod
                    LIMIT @MaxBookingCancellation
                  ) alias;
            """, new { UserId = userId.Value, bookingOptions.Value.BookingCancellationPeriod, bookingOptions.Value.MaxBookingCancellation });
        if (!shouldBlock) return Result.Ok();
        
        var blockRes = user.Block(timeProvider, bookingOptions.Value.ViolationCancellationPolicyBlockDuration);
        if(blockRes.IsFailed) return blockRes;
        
        await dbContext.SaveChangesAsync();
        return Result.Ok();
    }
}