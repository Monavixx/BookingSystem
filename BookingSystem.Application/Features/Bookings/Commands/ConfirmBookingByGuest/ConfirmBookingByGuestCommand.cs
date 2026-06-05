using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.ConfirmBookingByGuest;

public record ConfirmBookingByGuestCommand(Guid BookingId) : IRequest<Result>;