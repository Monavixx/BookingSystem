using BookingSystem.Application.Features.Auth.Commands.LogIn;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Tests.Application.Features.Auth;

public class LogInHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task When_CredentialsAreValid_ByUsername_ReturnsSuccessAndCreatesSession()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var password = "super-secret";
        var ph = Scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = await Users.CreateUserAsync(b => b
            .WithUsername("login_user")
            .WithEmail("login_user@example.com")
            .WithPasswordHash(ph.HashPassword(password)));

        // act
        var res = await Mediator.Send(new LogInCommand(null, user.Username.Value, password),
            TestContext.Current.CancellationToken);

        // assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Id.Should().Be(user.Id.Value);

        // ensure session persisted
        NewScope();
        var updated = await DbContext.Users.Include(u => u.Sessions)
            .SingleAsync(u => u.Id == user.Id, TestContext.Current.CancellationToken);
        updated.Sessions.Should().HaveCount(1);
    }

    [Fact]
    public async Task When_CredentialsAreValid_ByEmail_ReturnsSuccessAndCreatesSession()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var password = "another-secret";
        var ph = Scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = await Users.CreateUserAsync(b => b
            .WithUsername("email_user")
            .WithEmail("email_user@example.com")
            .WithPasswordHash(ph.HashPassword(password)));

        var res = await Mediator.Send(new LogInCommand(user.Email.Value, null, password), TestContext.Current.CancellationToken);

        res.IsSuccess.Should().BeTrue();
        res.Value.Id.Should().Be(user.Id.Value);

        NewScope();
        var updated = await DbContext.Users.Include(u => u.Sessions)
            .SingleAsync(u => u.Id == user.Id, TestContext.Current.CancellationToken);
        updated.Sessions.Should().HaveCount(1);
    }

    [Fact]
    public async Task When_PasswordIsInvalid_ReturnsInvalidCredentials()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var password = "valid-pass";
        var ph = Scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = await Users.CreateUserAsync(b => b
            .WithUsername("badpass_user")
            .WithEmail("badpass_user@example.com")
            .WithPasswordHash(ph.HashPassword(password)));

        var res = await Mediator.Send(new LogInCommand(null, user.Username.Value, "wrong-password"), TestContext.Current.CancellationToken);

        res.ShouldContain(LogInErrors.InvalidCredentials);
    }

    [Fact]
    public async Task When_NoIdentifierProvided_ReturnsIdentifierMissing()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var res = await Mediator.Send(new LogInCommand(null, null, "whatever"), TestContext.Current.CancellationToken);
        res.ShouldContain<ValidationError>();
    }
}


