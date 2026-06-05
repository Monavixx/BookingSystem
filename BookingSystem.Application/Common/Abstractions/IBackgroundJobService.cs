using System.Linq.Expressions;

namespace BookingSystem.Application.Common.Abstractions;

public interface IBackgroundJobService
{
    string Enqueue(Expression<Action> methodCall);

    string Enqueue<T>(Expression<Action<T>> methodCall);
    string Schedule(Expression<Action> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

    void AddOrUpdateRecurring(
        string jobId,
        Expression<Action> recurringJobExpression,
        Func<string> cronExpression);

    void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Action<T>> recurringJobExpression,
        Func<string> cronExpression);

    void Delete(string jobId);
}