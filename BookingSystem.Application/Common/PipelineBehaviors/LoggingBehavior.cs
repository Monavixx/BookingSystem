using System.Diagnostics;
using System.Reflection;
using BookingSystem.Application.Common.Attributes;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Common.PipelineBehaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}: {@Request}", requestName, Sanitize(request));
        
        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();
        
        logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms",
            requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }

    private Dictionary<string, object?>? Sanitize(object? obj)
    {
        if (obj is null) return null;
        var result = new Dictionary<string, object?>();

        var props = obj.GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.GetCustomAttribute<SensitiveCommandPropertyAttribute>() is not null)
                continue;
            result[prop.Name] = prop.GetValue(obj);
        }

        return result;
    }
}