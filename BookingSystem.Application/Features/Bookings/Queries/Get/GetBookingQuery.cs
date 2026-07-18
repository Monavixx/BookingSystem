using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Features.Bookings.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Queries.Get;

public sealed record GetBookingQuery(Guid BookingId) : IRequest<Result<BookingDto>>, IRequireActiveUser;