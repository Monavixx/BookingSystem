using BookingSystem.Application.Features.Bookings.DTOs;
using BookingSystem.Domain.Bookings.ValueObjects;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Queries.GetAll;

public sealed record GetAllBookingsQuery(
    Guid? RestaurantId = null,
    int? TableNumber = null,
    BookingStatus? Status = null,
    DateTimeOffset? Start = null,
    DateTimeOffset? End = null,
    TimeFilterMethod TimeFilterMethod = TimeFilterMethod.In,
    Guid? GuestId = null,
    FilterMode FilterMode = FilterMode.All
) : IRequest<Result<ICollection<BookingDto>>>;

public enum TimeFilterMethod
{
    In,
    NotOverlapping,
    Overlapping
}

public enum FilterMode
{
    All,
    Any
}