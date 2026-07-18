using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Features.Bookings.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Create;

public sealed record CreateBookingCommand(
    int GuestCount,
    Guid RestaurantId,
    int? TableNumber,
    DateTimeOffset ScheduledAt)
    : IRequest<Result<BookingDto>>, IRequireActiveUser;