using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Complete.CompleteByManager;

public sealed record CompleteBookingByManagerCommand(Guid BookingId) : IRequest<Result>;