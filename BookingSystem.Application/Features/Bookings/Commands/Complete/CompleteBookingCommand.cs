using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.Complete;

public sealed record CompleteBookingCommand(Guid BookingId) : IRequest<Result>, IRequireActiveUser;
