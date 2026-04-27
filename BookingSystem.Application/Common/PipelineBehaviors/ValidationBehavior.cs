using BookingSystem.Domain.Common.Errors;
using FluentResults;
using FluentValidation;
using MediatR;

namespace BookingSystem.Application.Common.PipelineBehaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> 
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken)).ToArray();
        if (validationTasks.Length == 0)
            return await next(cancellationToken);
        
        var results = await Task.WhenAll(validationTasks);
        if (results.All(r => r.IsValid)) return await next(cancellationToken);
        
        var errors = results.SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .Select(e =>
            {
                var error = new ValidationError(e.ErrorCode, e.ErrorMessage);
                error.Metadata.Add("PropertyName", e.PropertyName);
                return error;
            })
            .ToArray();
        
        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition().IsAssignableTo(typeof(Result<>)))
        {
            var failMethod = typeof(Result).GetMethod("Fail", 1, [typeof(IEnumerable<IError>)])
                !.MakeGenericMethod(resultType.GenericTypeArguments[0]);
            return (TResponse)failMethod.Invoke(null, [errors])!;
        }
        if (resultType == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(errors);
        }

        throw new InvalidOperationException($"Unsupported TResponse type: {resultType}");
    }
}
// TODO: password validation