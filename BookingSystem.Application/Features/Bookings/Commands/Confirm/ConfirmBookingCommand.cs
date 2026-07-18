using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Confirm;

public sealed record ConfirmBookingCommand(Guid BookingId) : IRequest<Result>, IRequireActiveUser;