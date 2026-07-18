using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.Block;

public sealed record BlockUserCommand(Guid UserId, TimeSpan? Duration) : IRequest<Result>, IRequireActiveUser;