using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.DTOs;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Bookings.Queries.Get;

public class GetBookingHandler(AppDbContext dbContext, IReadOnlyCurrentUserService currentUserService)
    : IRequestHandler<GetBookingQuery, Result<BookingDto>>
{
    public async Task<Result<DTOs.BookingDto>> Handle(GetBookingQuery request, CancellationToken cancellationToken)
    {
        var res = await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<BookingDto>(
            """
            SELECT b.status as "Status", 
                   b.end_time as "End",
                   b.start_time as "Start",
                   b.restaurant_id as "RestaurantId",
                   b.table_number as "TableNumber",
                   b.guest_count as "GuestCount",
                   b.guest_id as "GuestId",
                   r.owner_id as "RestaurantOwnerId"
                   FROM bookings b
            INNER JOIN restaurants r ON b.restaurant_id = r.id
            WHERE b.id = @BookingId
            LIMIT 1
            """, new { request.BookingId });
        if (res is null)
            return Result.Fail<DTOs.BookingDto>(BookingErrors.NotFound);

        var user = await currentUserService.GetAsync();
        if (user is null || !CanAccess(user, res))
            return Result.Fail<DTOs.BookingDto>(BookingErrors.AccessDenied);

        return new DTOs.BookingDto(
            Id: request.BookingId,
            GuestId: res.GuestId,
            GuestCount: res.GuestCount,
            RestaurantId: res.RestaurantId,
            TableNumber: res.TableNumber,
            Status: res.Status,
            Start: res.Start,
            End: res.End);
    }

    private static bool CanAccess(CachedUser user, BookingDto booking) =>
        user.Role == UserRole.Admin ||
        user.Id == booking.GuestId ||
        (user.Role == UserRole.Manager && user.Id == booking.RestaurantOwnerId);


    private sealed record BookingDto
    {
        public Guid GuestId { get; init; }
        public int GuestCount { get; init; }
        public Guid RestaurantId { get; init; }
        public int TableNumber { get; init; }
        public BookingStatus Status { get; init; }
        public DateTimeOffset Start { get; init; }
        public DateTimeOffset End { get; init; }
        public Guid RestaurantOwnerId { get; init; }
    }
}
