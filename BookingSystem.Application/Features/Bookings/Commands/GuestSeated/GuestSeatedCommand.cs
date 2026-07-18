using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Bookings.Commands.GuestSeated;

public sealed record GuestSeatedCommand(Guid BookingId) : IRequest<Result>, IRequireActiveUser;