using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Queries.Get;

public sealed record GetBookingQuery(Guid BookingId) : IRequest<Result<GetBookingResponse>>;