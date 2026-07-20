using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Users.Queries.GetUsers;

public class GetUsersHandler (AppDbContext dbContext, TimeProvider timeProvider, ICurrentUserService currentUserService):
    IRequestHandler<GetUsersQuery, Result<IEnumerable<UserResponse>>>
{
    public async Task<Result<IEnumerable<UserResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .AsNoTracking()
            .Where(u=>u.Id != currentUserService.GetRequiredUserId());

        if (request.OlderThan.HasValue)
        {
            var birthdate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)
                .AddYears(-request.OlderThan.Value);
            query = query.Where(u => u.BirthDate.Value <= birthdate);
        }
        if (request.YoungerThan.HasValue)
        {
            var birthdate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)
                .AddYears(-request.YoungerThan.Value);
            query = query.Where(u => u.BirthDate.Value >= birthdate);
        }

        if (request.BookingCountGreaterThan.HasValue)
            query = query.Where(u =>
                dbContext.Bookings.Count(b => b.GuestId == u.Id) >= request.BookingCountGreaterThan.Value);
        if (request.BookingCountLessThan.HasValue)
            query = query.Where(u =>
                dbContext.Bookings.Count(b => b.GuestId == u.Id) <= request.BookingCountLessThan.Value);
        if(request.IsBlocked.HasValue)
            query = query.Where(u => u.IsBlocked == request.IsBlocked.Value);
        if (request.RestaurantUserBeenTo.HasValue)
            query = query.Where(u => dbContext.Bookings.Any(b =>
                b.GuestId == u.Id && b.RestaurantId == new RestaurantId(request.RestaurantUserBeenTo.Value)
                && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Seated)));
        if(request.RestaurantUserIsAt.HasValue)
            query = query.Where(u => dbContext.Bookings.Any(b =>
                b.GuestId == u.Id && b.RestaurantId == new RestaurantId(request.RestaurantUserIsAt.Value)
                && b.Status == BookingStatus.Seated));

        return await query.Select(UserResponse.Projection).ToArrayAsync(cancellationToken);
    }
}