using BookingSystem.Application.Common.Factories;
using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Common.Errors;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingSystem.Application.Common.PipelineBehaviors;

public class DbExceptionHandlingBehavior<TRequest, TResponse> 
    (ConstraintErrorRegistryBase constraintErrorRegistryBase)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            var error =
                constraintErrorRegistryBase.TryResolve(pgEx.TableName!, pgEx.ConstraintName!) ?? new InternalServerError(
                    "Database.ConstraintViolation",
                    "A database constraint was violated"
                );
            var res = ResultFactory.CreateFailure<TResponse>([error]);
            return res;
        }
    }
}