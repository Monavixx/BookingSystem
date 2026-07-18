using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Commands.MakeManager;

public record MakeManagerCommand(Guid UserId) : IRequest<Result>, IRequireActiveUser;