using BookingSystem.Application.Features.Users.Commands.Block;
using BookingSystem.Domain.Users.Errors;
using FluentAssertions;

namespace BookingSystem.Tests.Application.Features.Users.Commands;

public class BlockUserHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    protected override async Task InitAsync()
    {
        var admin = 
            await Users.CreateAdminAsync("eprerereee", "fwrfnrwfnrihion@mmm.rre", "+79264875151");
        SetCurrentUser(admin);
    }

    [Fact]
    public async Task When_UserDoesNotExist_ReturnsNotExist()
    {
        var userId = Guid.NewGuid();
        var res = await Mediator.Send(new BlockUserCommand(userId, TimeSpan.FromMinutes(5)));
        res.ShouldContain(UserErrors.NotFound);
    }

    [Fact]
    public async Task When_GuestBeingBlocked_BlocksUser()
    {
        var user = await Users.CreateGuestAsync();
        var res = await Mediator.Send(new BlockUserCommand(user.Id.Value, TimeSpan.FromMinutes(5)));
        res.IsSuccess.Should().BeTrue();
        NewScope();
        
        var updatedUser = await DbContext.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.IsBlocked.Should().BeTrue();
        updatedUser.BlockedUntil.Should().BeCloseTo(FakeTime.GetUtcNow().AddMinutes(5), TimeSpan.FromMilliseconds(1));
    }
    
    [Fact]
    public async Task When_AdminBeingBlocked_ReturnsForbidden()
    {
        var user = await Users.CreateAdminAsync();
        var res = await Mediator.Send(new BlockUserCommand(user.Id.Value, TimeSpan.FromMinutes(5)));
        res.ShouldContain(UserErrors.AdminCannotBeBlocked);
    }
}