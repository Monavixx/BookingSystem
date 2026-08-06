using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.Factories;
using BookingSystem.Domain.Users.Errors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Common.PipelineBehaviors;

public class ActiveUserCheckBehavior<TRequest, TResponse>(IReadOnlyCurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequireActiveUser)
        {
            var user = await currentUserService.GetAsync();
            if (user is null)
                return ResultFactory.CreateFailure<TResponse>([UserErrors.NotFound]);
            if (user.IsBlocked)
                return ResultFactory.CreateFailure<TResponse>([UserErrors.IsBlocked]);
        }
        return await next(cancellationToken);
    }
}
