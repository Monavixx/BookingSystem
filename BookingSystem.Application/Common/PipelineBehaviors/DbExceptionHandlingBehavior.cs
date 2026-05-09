using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.Factories;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingSystem.Application.Common.PipelineBehaviors;

public class DbExceptionHandlingBehavior<TRequest, TResponse> (IConstraintViolationMapper constraintViolationMapper) : IPipelineBehavior<TRequest, TResponse>
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
                constraintViolationMapper.MapConstraintViolation(pgEx.SqlState, pgEx.ConstraintName!, pgEx.TableName!);
            var res = ResultFactory.CreateFailure<TResponse>([error]);
            return res;
        }
    }
}