using FluentResults;

namespace BookingSystem.Application.Common.Factories;

public static class ResultFactory
{
    public static TResult CreateFailure<TResult>(IEnumerable<IError> errors)
    {
        var resultType = typeof(TResult);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition().IsAssignableTo(typeof(Result<>)))
        {
            var failMethod = typeof(Result).GetMethod("Fail", 1, [typeof(IEnumerable<IError>)])
                !.MakeGenericMethod(resultType.GenericTypeArguments[0]);
            return (TResult)failMethod.Invoke(null, [errors])!;
        }
        if (resultType == typeof(Result))
        {
            return (TResult)(object)Result.Fail(errors);
        }

        throw new InvalidOperationException($"Unsupported TResponse type: {resultType}");
    }
}