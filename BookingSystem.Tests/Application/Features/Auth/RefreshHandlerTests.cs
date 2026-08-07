using BookingSystem.Application.Features.Auth.Commands.Refresh;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Domain.Users.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Tests.Application.Features.Auth;

public class RefreshHandlerTests(IntegrationTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    [Fact]
    public async Task ValidRefreshToken_ReturnsNewAccessAndRefreshTokens()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user = await Users.CreateGuestAsync();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var oldRefreshToken = refreshTokenService.GenerateRefreshToken();
        user.AddSession(oldRefreshToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // act
        var res = await Mediator.Send(
            new RefreshCommand(oldRefreshToken.ToString()),
            TestContext.Current.CancellationToken);

        // assert
        res.IsSuccess.Should().BeTrue();
        res.Value.AccessToken.Should().NotBeNullOrWhiteSpace();
        res.Value.RefreshToken.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidRefreshToken_UpdatesSessionWithNewRefreshToken()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user = await Users.CreateGuestAsync();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var oldRefreshToken = refreshTokenService.GenerateRefreshToken();
        user.AddSession(oldRefreshToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var oldTokenString = oldRefreshToken.ToString();

        // act
        await Mediator.Send(
            new RefreshCommand(oldTokenString),
            TestContext.Current.CancellationToken);

        // assert
        NewScope();
        var session = await DbContext.Sessions
            .FirstOrDefaultAsync(s => s.UserId == user.Id, TestContext.Current.CancellationToken);

        session.Should().NotBeNull();
        session.RefreshToken.ToString().Should().NotBe(oldTokenString);
    }

    [Fact]
    public async Task InvalidRefreshToken_ReturnsFailed()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var invalidToken = Convert.ToBase64String(new byte[32]);

        // act
        var res = await Mediator.Send(
            new RefreshCommand(invalidToken),
            TestContext.Current.CancellationToken);

        // assert
        res.IsFailed.Should().BeTrue();
        res.ShouldContain(SessionErrors.NotFound);
    }

    [Fact]
    public async Task RefreshTokenNotInDatabase_ReturnsFailed()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();
        var unusedToken = refreshTokenService.GenerateRefreshToken();

        // act
        var res = await Mediator.Send(
            new RefreshCommand(unusedToken.ToString()),
            TestContext.Current.CancellationToken);

        // assert
        res.IsFailed.Should().BeTrue();
        res.ShouldContain(SessionErrors.NotFound);
    }

    [Fact]
    public async Task SessionWithoutUser_ReturnsFailed()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user = await Users.CreateGuestAsync();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var refreshToken = refreshTokenService.GenerateRefreshToken();
        user.AddSession(refreshToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Delete the user to simulate orphaned session
        DbContext.Users.Remove(user);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        NewScope();

        // act
        var res = await Mediator.Send(
            new RefreshCommand(refreshToken.ToString()),
            TestContext.Current.CancellationToken);

        // assert
        res.IsFailed.Should().BeTrue();
        res.ShouldContainOneOf(UserErrors.NotFound, SessionErrors.NotFound);
    }

    [Fact]
    public async Task MultipleSessionsForSameUser_OnlyRefreshesValidSession()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user = await Users.CreateGuestAsync();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var firstToken = refreshTokenService.GenerateRefreshToken();
        var secondToken = refreshTokenService.GenerateRefreshToken();
        user.AddSession(firstToken);
        user.AddSession(secondToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var firstTokenString = firstToken.ToString();
        var sessionsCountBefore = await DbContext.Sessions
            .CountAsync(s => s.UserId == user.Id, TestContext.Current.CancellationToken);

        // act
        var res = await Mediator.Send(
            new RefreshCommand(firstTokenString),
            TestContext.Current.CancellationToken);

        // assert
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var sessionsCountAfter = await DbContext.Sessions
            .CountAsync(s => s.UserId == user.Id, TestContext.Current.CancellationToken);

        sessionsCountAfter.Should().Be(sessionsCountBefore);

        var allSessions = await DbContext.Sessions
            .Where(s => s.UserId == user.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        var refreshedSession = allSessions
            .FirstOrDefault(s => s.RefreshToken.ToString() != firstTokenString);

        refreshedSession.Should().NotBeNull();
    }

    [Fact]
    public async Task GeneratedAccessTokenIsValidForUser()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user = await Users.CreateGuestAsync();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var refreshToken = refreshTokenService.GenerateRefreshToken();
        user.AddSession(refreshToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // act
        var res = await Mediator.Send(
            new RefreshCommand(refreshToken.ToString()),
            TestContext.Current.CancellationToken);

        // assert
        res.IsSuccess.Should().BeTrue();
        res.Value.AccessToken.Should().NotBeNullOrEmpty();
        // The JWT token should be a non-empty string (format verification: JWT has 3 parts separated by dots)
        res.Value.AccessToken.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public async Task RefreshTokenGenerationWithDifferentUsers_CreatesIndependentSessions()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user1 = await Users.CreateGuestAsync("user1", "user1@test.com", "+11111111111");
        var user2 = await Users.CreateGuestAsync("user2", "user2@test.com", "+22222222222");
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var token1 = refreshTokenService.GenerateRefreshToken();
        var token2 = refreshTokenService.GenerateRefreshToken();
        user1.AddSession(token1);
        user2.AddSession(token2);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // act
        var res1 = await Mediator.Send(
            new RefreshCommand(token1.ToString()),
            TestContext.Current.CancellationToken);

        NewScope();

        var res2 = await Mediator.Send(
            new RefreshCommand(token2.ToString()),
            TestContext.Current.CancellationToken);

        // assert
        res1.IsSuccess.Should().BeTrue();
        res2.IsSuccess.Should().BeTrue();
        res1.Value.AccessToken.Should().NotBe(res2.Value.AccessToken);
    }

    [Fact]
    public async Task ReusingSameRefreshTokenAfterRefresh_Fails()
    {
        // arrange
        FakeTime.AdjustTime(DateTimeOffset.UtcNow);
        var user = await Users.CreateGuestAsync();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        var oldToken = refreshTokenService.GenerateRefreshToken();
        user.AddSession(oldToken);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var oldTokenString = oldToken.ToString();

        // act - First refresh should succeed
        var firstRefresh = await Mediator.Send(
            new RefreshCommand(oldTokenString),
            TestContext.Current.CancellationToken);

        NewScope();

        // Act - Try to reuse the old token
        var secondRefresh = await Mediator.Send(
            new RefreshCommand(oldTokenString),
            TestContext.Current.CancellationToken);

        // assert
        firstRefresh.IsSuccess.Should().BeTrue();
        secondRefresh.IsFailed.Should().BeTrue();
        secondRefresh.ShouldContain(SessionErrors.NotFound);
    }

    [Fact]
    public async Task MalformedRefreshToken_ReturnsFailed()
    {
        // arrange
        var malformedToken = "not-a-valid-base64!!!";

        // act & assert
        var res = await Mediator.Send(
            new RefreshCommand(malformedToken),
            TestContext.Current.CancellationToken);

        res.ShouldContain(RefreshTokenErrors.Invalid);
    }
}









