using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Complete.CompleteBySystem;

public sealed record CompleteBookingBySystemCommand(Guid BookingId) : IRequest<Result>;