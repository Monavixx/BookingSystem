using BookingSystem.Application.Features.Auth.Commands.SignUp;
using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Auth;

public class SignUpHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task When_DataIsValid_ReturnsSuccessAndCreatesUserAndSession()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);

        var password = "strong-password";
        var username = "new_user";
        var email = "new_user@example.com";
        var firstName = "New";
        var lastName = "User";
        var phone = "+1234567890";
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25));

        var res = await Mediator.Send(new SignUpCommand(username, password, email, firstName, lastName, phone, dob),
            TestContext.Current.CancellationToken);

        res.IsSuccess.Should().BeTrue();
        res.Value.Id.Should().NotBeEmpty();
        res.Value.AuthTokens.AccessToken.Should().NotBeNullOrEmpty();
        res.Value.AuthTokens.RefreshToken.Should().NotBeNull();

        NewScope();
        var updated = await DbContext.Users.Include(u => u.Sessions)
            .SingleAsync(u => u.Id == new UserId(res.Value.Id), TestContext.Current.CancellationToken);

        updated.Sessions.Should().HaveCount(1);
        updated.Sessions.Single().RefreshToken.ToString()
            .Should().Be(res.Value.AuthTokens.RefreshToken.ToString());
    }

    [Fact]
    public async Task When_FirstNameIsEmpty_ReturnsFirstNameEmpty()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);

        var res = await Mediator.Send(new SignUpCommand(
            "user2", "pw-123456", "user2@example.com", string.Empty, "Last", "+123", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))),
            TestContext.Current.CancellationToken);

        res.ShouldContain<ValidationError>();
    }

    [Fact]
    public async Task When_BirthdateTooYoung_ReturnsBirthdateTooYoung()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);

        var tooYoungDob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10));

        var res = await Mediator.Send(new SignUpCommand(
            "young_user", "password123", "young@example.com", "Young", "User", "+123", tooYoungDob),
            TestContext.Current.CancellationToken);

        res.ShouldContain(UserErrors.Birthdate.TooYoung);
    }

    [Fact]
    public async Task When_UsernameAlreadyExists_ReturnsUsernameAlreadyInUse()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);

        var existing = await Users.CreateUserAsync(b => b
            .WithUsername("dup_user")
            .WithEmail("unique_email@example.com")
            .WithPhoneNumber("+70000000001"));

        var res = await Mediator.Send(new SignUpCommand(
            existing.Username.Value, "other-pass", "other@example.com", "First", "Last", "+70000000002", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))),
            TestContext.Current.CancellationToken);

        res.ShouldContain(UserErrors.Username.AlreadyInUse);
    }

    [Fact]
    public async Task When_EmailAlreadyExists_ReturnsEmailAlreadyInUse()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);

        var existing = await Users.CreateUserAsync(b => b
            .WithUsername("unique_user_2")
            .WithEmail("dup_email@example.com")
            .WithPhoneNumber("+70000000003"));

        var res = await Mediator.Send(new SignUpCommand(
            "another_user", "other-pass", existing.Email.Value, "First", "Last", "+70000000004", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))),
            TestContext.Current.CancellationToken);

        res.ShouldContain(UserErrors.Email.AlreadyInUse);
    }

    [Fact]
    public async Task When_PhoneNumberAlreadyExists_ReturnsPhoneNumberAlreadyInUse()
    {
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);

        var existing = await Users.CreateUserAsync(b => b
            .WithUsername("unique_user_3")
            .WithEmail("unique3@example.com")
            .WithPhoneNumber("+70000000005"));

        var res = await Mediator.Send(new SignUpCommand(
            "another_user_3", "other-pass", "other3@example.com", "First", "Last", existing.PhoneNumber.Value, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30))),
            TestContext.Current.CancellationToken);

        res.ShouldContain(UserErrors.PhoneNumber.AlreadyInUse);
    }
}

