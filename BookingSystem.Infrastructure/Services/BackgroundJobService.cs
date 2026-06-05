using System.Linq.Expressions;
using BookingSystem.Application.Common.Abstractions;
using Hangfire;

namespace BookingSystem.Infrastructure.Services;

public class BackgroundJobService(IBackgroundJobClientV2 backgroundJobClient, IRecurringJobManagerV2 recurringJobManager) : IBackgroundJobService
{
    public string Enqueue(Expression<Action> methodCall)
    => backgroundJobClient.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Action<T>> methodCall)
        => backgroundJobClient.Enqueue(methodCall);
    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
        => backgroundJobClient.Schedule(methodCall, delay);
    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    => backgroundJobClient.Schedule(methodCall, delay);

    public string Schedule(Expression<Action> methodCall, DateTimeOffset scheduleAt)
        => backgroundJobClient.Schedule(methodCall, scheduleAt);

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset scheduleAt)
        => backgroundJobClient.Schedule(methodCall, scheduleAt);

    public void AddOrUpdateRecurring(
        string jobId,
        Expression<Action> recurringJobExpression,
        Func<string> cronExpression)
    => recurringJobManager.AddOrUpdate(jobId, recurringJobExpression, cronExpression);

    public void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Action<T>> recurringJobExpression,
        Func<string> cronExpression)
        => recurringJobManager.AddOrUpdate(jobId, recurringJobExpression, cronExpression);
    
    public void Delete(string jobId)
    => backgroundJobClient.Delete(jobId);
    
}