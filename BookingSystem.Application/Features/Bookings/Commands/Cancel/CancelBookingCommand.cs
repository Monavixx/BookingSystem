using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Cancel;

public sealed record CancelBookingCommand(Guid BookingId, bool IsGuestRequest = false) : IRequest<Result>, IRequireActiveUser;