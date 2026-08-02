using BookingSystem.Application.Features.Restaurants.Commands.DeleteTable;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Tests.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests.Application.Features.Restaurants.Commands;

public class DeleteTablesHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private Base5Users _users = null!;
    private Restaurant _restaurant1 = null!, _restaurant2 = null!;

    protected override async ValueTask InitAsync()
    {
        _users = await Users.CreateBase5Async();
        var restaurants = await Restaurants.CreateRestaurants(c =>
        {
            c.AddRestaurant(_users.Manager,
                    (1, 4), (2, 4), (3, 20))
                .AddRestaurant(_users.AnotherManager, (1, 2),
                    (2, 3), (3, 8), (4, 3));
        });
        (_restaurant1, _restaurant2) = (restaurants[0], restaurants[1]);
    }

    [Fact]
    public async Task When_Manager_ProvidedTableExists_ShouldDeleteTable()
    {
        SetCurrentUser(_users.Manager);
        var res = await Mediator.Send(new DeleteTablesCommand(
        [
            new TableId(_restaurant1.Id, 2),
            new TableId(_restaurant1.Id, 3)
        ]), TestContext.Current.CancellationToken);
        res.ShouldBeSuccess();
        NewScope();

        (await DbContext.Tables.Where(t => t.RestaurantId == _restaurant1.Id && t.TableNumber == 2)
                .FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken))
            .Should().BeNull();
    }
    [Fact]
    public async Task When_Manager_ProvidedTableDoesNotExist_ShouldReturnError()
    {
        SetCurrentUser(_users.Manager);
        var res = await Mediator.Send(new DeleteTablesCommand(
        [
            new TableId(_restaurant1.Id, 2),
            new TableId(_restaurant1.Id, 99)
        ]), TestContext.Current.CancellationToken);
        res.ShouldContain(TableErrors.NotFound);
        // ensure the operation is atomic and no table was deleted
        NewScope();
        (await DbContext.Tables.Where(t => t.RestaurantId == _restaurant1.Id && t.TableNumber == 2)
                .FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken))
            .Should().NotBeNull();
    }
}
