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
        
        query = request.FilterMode switch
        {
            FilterMode.All => ApplyAllFilters(query, request),
            FilterMode.Any => ApplyAnyFilters(query, request),
            _ => query
        };

        var bookings = await query.AsAsyncEnumerable().Select(x => new BookingDto(x))
            .ToArrayAsync(cancellationToken);
        return bookings;
    }

    private IQueryable<Booking> ApplyAuthorization(IQueryable<Booking> query, User user)
    {
        query = user.Role switch
        {
            UserRole.Admin => query,
            UserRole.Manager => query.Where(b => b.Table.Restaurant.OwnerId == user.Id || b.GuestId == user.Id),
            UserRole.Guest => query.Where(b => b.GuestId == user.Id),
            _ => throw new InvalidOperationException($"Unknown user role: {user.Role}")
        };
        
        return query;
    }
    
    private static IQueryable<Booking> ApplyAllFilters(IQueryable<Booking> query, GetAllBookingsQuery request)
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
                    b.TimeSlot.Start >= request.Start.Value && b.TimeSlot.End <= request.End.Value),
                TimeFilterMethod.NotOverlapping => query.Where(b =>
                    b.TimeSlot.End <= request.Start.Value || b.TimeSlot.Start >= request.End.Value),
                TimeFilterMethod.Overlapping => query.Where(b =>
                    b.TimeSlot.Start < request.End.Value && b.TimeSlot.End > request.Start.Value),
                _ => query
            },
            { Start: not null, End: null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.Start >= request.Start.Value),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.End <= request.Start.Value),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.End > request.Start.Value),
                _ => query
            },
            { Start: null, End: not null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.End <= request.End.Value),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.Start >= request.End.Value),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.Start < request.End.Value),
                _ => query
            },
            _ => query
        };

        if (request.GuestId.HasValue)
            query = query.Where(b => b.GuestId == new UserId(request.GuestId.Value));

        return query;
    }

    private static IQueryable<Booking> ApplyAnyFilters(IQueryable<Booking> query, GetAllBookingsQuery request)
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
                    b.TimeSlot.Start >= request.Start.Value && b.TimeSlot.End <= request.End.Value),
                TimeFilterMethod.NotOverlapping => query.Where(b =>
                    b.TimeSlot.End <= request.Start.Value || b.TimeSlot.Start >= request.End.Value),
                TimeFilterMethod.Overlapping => query.Where(b =>
                    b.TimeSlot.Start < request.End.Value && b.TimeSlot.End > request.Start.Value),
                _ => query
            },
            { Start: not null, End: null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.Start >= request.Start.Value),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.End <= request.Start.Value),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.End > request.Start.Value),
                _ => query
            },
            { Start: null, End: not null } => request.TimeFilterMethod switch
            {
                TimeFilterMethod.In => query.Where(b => b.TimeSlot.End <= request.End.Value),
                TimeFilterMethod.NotOverlapping => query.Where(b => b.TimeSlot.Start >= request.End.Value),
                TimeFilterMethod.Overlapping => query.Where(b => b.TimeSlot.Start < request.End.Value),
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