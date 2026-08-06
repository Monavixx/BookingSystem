using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Tests.Builders;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace BookingSystem.Tests.Application.Common.PipelineBehaviors;

public class ActiveUserCheckBehaviorTests
{
    private readonly Mock<IReadOnlyCurrentUserService> _currentUserServiceMock = new();
    private readonly TimeProvider _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task When_UserIsBlocked_ShouldReturnError()
    {
        var user = new UserBuilder()
            .WithIsBlocked(true)
            .WithBlockedUntil(_timeProvider.GetUtcNow().AddDays(1))
            .Build(_timeProvider);
        _currentUserServiceMock.Setup(s => s.GetAsync()).ReturnsAsync(CachedUser.FromUser(user));
        var behavior = new ActiveUserCheckBehavior<TestCommand, Result>(_currentUserServiceMock.Object);
        bool nextCalled = false;
        var res = await behavior.Handle(new TestCommand(), c =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        }, CancellationToken.None);
        res.ShouldContain(UserErrors.IsBlocked);
        nextCalled.Should().BeFalse();
    }

    public record TestCommand : IRequest<Result>, IRequireActiveUser;
}
