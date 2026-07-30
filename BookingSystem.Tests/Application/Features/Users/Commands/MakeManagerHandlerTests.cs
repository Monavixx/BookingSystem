using BookingSystem.Application.Features.Users.Commands.MakeManager;
using BookingSystem.Domain.Users;
using FluentAssertions;

namespace BookingSystem.Tests.Application.Features.Users.Commands;

public class MakeManagerHandlerTests(PostgresTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task HappyPath_ShouldMakeManager()
    {
        var admin = await Users.CreateAdminAsync();
        var guest = await Users.CreateGuestAsync();
        SetCurrentUser(admin);
        NewScope();

        var res = await Mediator.Send(new MakeManagerCommand(guest.Id.Value), TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue();

        NewScope();
        var manager = await DbContext.Managers.FindAsync([guest.Id], TestContext.Current.CancellationToken);
        manager.Should().NotBeNull();
        var managerUser = await DbContext.Users.FindAsync([guest.Id], TestContext.Current.CancellationToken);
        managerUser.Should().NotBeNull();
        managerUser.Role.Should().Be(UserRole.Manager);
    }
}
