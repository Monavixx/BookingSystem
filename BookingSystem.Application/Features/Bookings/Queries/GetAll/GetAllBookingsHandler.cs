using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Features.Bookings.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Bookings.Queries.GetAll;

public class GetAllBookingsHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<GetAllBookingsQuery, Result<ICollection<BookingDto>>>
{
    public async Task<Result<ICollection<BookingDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetUserAsync();
        if (user is null) return BookingErrors.AccessDenied;
        var query = ApplyAuthorization(
            dbContext.Bookings
                .AsNoTracking()
                .OrderByDescending(b => b.TimeSlot.Start), user);

        var start = request.Start?.ToUniversalTime();
        var end = request.End?.ToUniversalTime();
        query = request.FilterMode switch
        {
            FilterMode.All => ApplyAllFilters(query, request, start, end),
            FilterMode.Any => ApplyAnyFilters(query, request, start, end),
            _ => query
        };

        var bookings = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .AsAsyncEnumerable()
            .Select(x => new BookingDto(x))
            .ToArrayAsync(cancellationToken);
        return bookings;
    }

    private static IQueryable<Booking> ApplyAuthorization(IQueryable<Booking> query, User user)
        => user.Role switch
        {
            UserRole.Admin => query,
            UserRole.Manager => query.Where(b => b.Table.Restaurant.OwnerId == user.Id || b.GuestId == user.Id),
            UserRole.Guest => query.Where(b => b.GuestId == user.Id),
            _ => throw new InvalidOperationException($"Unknown user role: {user.Role}")
        };
    
    private static IQueryable<Booking> ApplyAllFilters(IQueryable<Booking> query, GetAllBookingsQuery request,
        DateTimeOffset? start, DateTimeOffset? end)
    {
        if (request.RestaurantId.HasValue)
            query = query.Where(b => new RestaurantId(request.RestaurantId.Value) == b.RestaurantId);

        if (request.TableNumber.HasValue)
            query = query.Where(b => b.TableNumber == request.TableNumber.Value);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        query = request switch
        {
            { Start: not null, End: not null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b =>
                    b.TimeSlot.Start >= start && b.TimeSlot.End <= end),
                TimeFilterMethod.NotOverlapping => query.Where(b =>
                    b.TimeSlot.End <= start || b.TimeSlot.Start >= end),
                TimeFilterMethod.Overlapping => query.Where(b =>
                    b.TimeSlot.Start < end && b.TimeSlot.End > start),
                _ => query
            },
            { Start: not null, End: null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.Start >= start),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.End <= start),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.End > start),
                _ => query
            },
            { Start: null, End: not null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.End <= end),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.Start >= end),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.Start < end),
                _ => query
            },
            _ => query
        };

        if (request.GuestId.HasValue)
            query = query.Where(b => b.GuestId == new UserId(request.GuestId.Value));

        return query;
    }

    private static IQueryable<Booking> ApplyAnyFilters(IQueryable<Booking> query, GetAllBookingsQuery request,
        DateTimeOffset? start, DateTimeOffset? end)
    {
        IQueryable<Booking>? result = null;
        
        if(request.RestaurantId.HasValue)
            result = query.Where(b => new RestaurantId(request.RestaurantId.Value) == b.RestaurantId);
        if (request.TableNumber.HasValue)
        {
            var filter = query.Where(b => b.TableNumber == request.TableNumber.Value);
            result = result is null ? filter : result.Concat(filter);
        }
        if(request.Status.HasValue)
        {
            var filter = query.Where(b => b.Status == request.Status.Value);
            result = result is null ? filter : result.Concat(filter);
        }

        var timeFilter = request switch
        {
            { Start: not null, End: not null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b =>
                    b.TimeSlot.Start >= start && b.TimeSlot.End <= end),
                TimeFilterMethod.NotOverlapping => query.Where(b =>
                    b.TimeSlot.End <= start || b.TimeSlot.Start >= end),
                TimeFilterMethod.Overlapping => query.Where(b =>
                    b.TimeSlot.Start < end && b.TimeSlot.End > start),
                _ => query
            },
            { Start: not null, End: null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.Start >= start),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.End <= start),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.End > start),
                _ => query
            },
            { Start: null, End: not null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.End <= end),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.Start >= end),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.Start < end),
                _ => query
            },
            _ => null
        };
        if(timeFilter is not null)
            result = result is null ? timeFilter : result.Concat(timeFilter);

        if (request.GuestId.HasValue)
        {
            var filter = query.Where(b => b.GuestId == new UserId(request.GuestId.Value));
            result = result is null ? filter : result.Concat(filter);
        }

        return result ?? query;
    }
}