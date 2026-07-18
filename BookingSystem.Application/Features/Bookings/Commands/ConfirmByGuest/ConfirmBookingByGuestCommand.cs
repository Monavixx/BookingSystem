using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.ConfirmByGuest;

public record ConfirmBookingByGuestCommand(Guid BookingId) : IRequest<Result>, IRequireActiveUser;