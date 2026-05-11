using BookingSystem.Application.Common.Factories;
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
                if (!string.IsNullOrWhiteSpace(e.PropertyName))
                    error.Metadata.Add("PropertyName", e.PropertyName);
                return error;
            });
        
        var resultType = typeof(TResponse);
        return ResultFactory.CreateFailure<TResponse>(errors);
    }
}
