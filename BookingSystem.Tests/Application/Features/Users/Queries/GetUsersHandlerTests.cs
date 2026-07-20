using BookingSystem.Application.Features.Users.Queries.GetUsers;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using FluentAssertions;

namespace BookingSystem.Tests.Application.Features.Users.Queries;

public class GetUsersHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private User _admin = null!;
    protected override async Task InitAsync()
    {
        _admin = await Users.CreateAdminAsync();
        SetCurrentUser(_admin);
    }
    
    [Fact]
    public async Task When_NoFilterProvided_ShouldReturnAllUsersExceptSelf()
    {
        await Users.CreateUsersAsync(b =>
        {
            b.New()
                .WithUsername("trmiogmeriog")
                .WithEmail("fgreiog@ggrg.afer")
                .WithPhoneNumber("+74448887575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-30).DateTime));
            b.New()
                .WithUsername("trmiogmriog")
                .WithEmail("fgreiog22e4@ggrg.afer")
                .WithPhoneNumber("+74448887574")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-15).DateTime));
            b.New()
                .WithUsername("trogmeriog")
                .WithEmail("fgre6546iog@ggrg.afer")
                .WithPhoneNumber("+74448787573")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-22).DateTime));
            b.New()
                .WithUsername("trmiogNFmerigmreoog")
                .WithEmail("fgreioGGRGQg@ggrg.afer")
                .WithPhoneNumber("+74448997575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-45).DateTime));
            b.New()
                .WithUsername("trmio484gmeriog")
                .WithEmail("fgreio950948g@ggrg.afer")
                .WithPhoneNumber("+74448886575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-28).DateTime));
        });
        NewScope();

        var queryRes = await Mediator.Send(new GetUsersQuery());
        queryRes.Errors.Should().BeEmpty();
        queryRes.Value.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(22, 4)]
    [InlineData(14, 5)]
    [InlineData(40, 1)]
    public async Task When_OlderThanFilterProvided_ShouldReturnAllUsersOverTheAgeExceptSelf(int olderThan, int expectedCount)
    {
        await Users.CreateUsersAsync(b =>
        {
            b.New()
                .WithUsername("trmiogmeriog")
                .WithEmail("fgreiog@ggrg.afer")
                .WithPhoneNumber("+74448887575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-30).DateTime));
            b.New()
                .WithUsername("trmiogmriog")
                .WithEmail("fgreiog22e4@ggrg.afer")
                .WithPhoneNumber("+74448887574")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-15).DateTime));
            b.New()
                .WithUsername("trogmeriog")
                .WithEmail("fgre6546iog@ggrg.afer")
                .WithPhoneNumber("+74448787573")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-22).DateTime));
            b.New()
                .WithUsername("trmiogNFmerigmreoog")
                .WithEmail("fgreioGGRGQg@ggrg.afer")
                .WithPhoneNumber("+74448997575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-45).DateTime));
            b.New()
                .WithUsername("trmio484gmeriog")
                .WithEmail("fgreio950948g@ggrg.afer")
                .WithPhoneNumber("+74448886575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-28).DateTime));
        });
        NewScope();
        
        var queryRes = await Mediator.Send(new GetUsersQuery(){OlderThan = olderThan});
        queryRes.Errors.Should().BeEmpty();
        queryRes.Value.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData(22, 2)]
    [InlineData(14, 0)]
    [InlineData(40, 4)]
    public async Task When_YoungerThanFilterProvided_ShouldReturnAllUsersUnderTheAgeExceptSelf(int youngerThan, int expectedCount)
    {
        await Users.CreateUsersAsync(b =>
        {
            b.New()
                .WithUsername("trmiogmeriog")
                .WithEmail("fgreiog@ggrg.afer")
                .WithPhoneNumber("+74448887575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-30).DateTime));
            b.New()
                .WithUsername("trmiogmriog")
                .WithEmail("fgreiog22e4@ggrg.afer")
                .WithPhoneNumber("+74448887574")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-15).DateTime));
            b.New()
                .WithUsername("trogmeriog")
                .WithEmail("fgre6546iog@ggrg.afer")
                .WithPhoneNumber("+74448787573")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-22).DateTime));
            b.New()
                .WithUsername("trmiogNFmerigmreoog")
                .WithEmail("fgreioGGRGQg@ggrg.afer")
                .WithPhoneNumber("+74448997575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-45).DateTime));
            b.New()
                .WithUsername("trmio484gmeriog")
                .WithEmail("fgreio950948g@ggrg.afer")
                .WithPhoneNumber("+74448886575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-28).DateTime));
        });
        NewScope();
        
        var queryRes = await Mediator.Send(new GetUsersQuery(){YoungerThan = youngerThan});
        queryRes.Errors.Should().BeEmpty();
        queryRes.Value.Should().HaveCount(expectedCount);
    }

    [Fact]
    public async Task
        When_BookingCountGreaterThanFilterProvided_ShouldReturnAllUsersWhoHasCreatedMoreBookingsThanProvidedNumberExceptSelf()
    {
        var users = await Users.CreateUsersAsync(b =>
        {
            b.New()
                .WithUsername("trmiogmeriog")
                .WithEmail("fgreiog@ggrg.afer")
                .WithPhoneNumber("+74448887575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-30).DateTime));
            b.New()
                .WithUsername("trmiogmriog")
                .WithEmail("fgreiog22e4@ggrg.afer")
                .WithPhoneNumber("+74448887574")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-15).DateTime));
            b.New()
                .WithUsername("trogmeriog")
                .WithEmail("fgre6546iog@ggrg.afer")
                .WithPhoneNumber("+74448787573")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-22).DateTime));
        });
        var manager = await Users.CreateManagerAsync();
        var restaurant = await Restaurants.CreateDefault(manager.Id.Value);
        await Bookings.CreateBookings(b => b
            .AddBooking(users[0], restaurant, 1, startTime: FakeTime.GetUtcNow().AddDays(-1), status: BookingStatus.Completed)
            .AddBooking(users[0], restaurant, 1, startTime: FakeTime.GetUtcNow().AddDays(1), status: BookingStatus.Confirmed)
            .AddBooking(users[0], restaurant, 1, startTime: FakeTime.GetUtcNow().AddDays(-2), status: BookingStatus.Canceled)
            .AddBooking(users[1], restaurant, 1, startTime: FakeTime.GetUtcNow().AddDays(2), status: BookingStatus.ConfirmedByGuest)
        );
        NewScope();
        var queryRes = await Mediator.Send(new GetUsersQuery(){BookingCountGreaterThan = 2});
        queryRes.Errors.Should().BeEmpty();
        queryRes.Value.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    public async Task
        When_RestaurantUserBeenToFilterProvided_ShouldReturnAllUsersWhoHasCompletedOrSeatedBookings(int restaurantIndex, int expectedCount)
    {
        var users = await Users.CreateUsersAsync(b =>
        {
            b.New()
                .WithUsername("trmiogmeriog")
                .WithEmail("fgreiog@ggrg.afer")
                .WithPhoneNumber("+74448887575")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-30).DateTime));
            b.New()
                .WithUsername("trmiogmriog")
                .WithEmail("fgreiog22e4@ggrg.afer")
                .WithPhoneNumber("+74448887574")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-15).DateTime));
            b.New()
                .WithUsername("trogmeriog")
                .WithEmail("fgre6546iog@ggrg.afer")
                .WithPhoneNumber("+74448787573")
                .WithDateOfBirth(DateOnly.FromDateTime(FakeTime.GetUtcNow().AddYears(-22).DateTime));
        });
        var manager = await Users.CreateManagerAsync();
        var anotherManager = await Users.CreateAnotherManagerAsync();
        var restaurants = await Restaurants.CreateRestaurants(b => b
            .AddRestaurant(manager, (1, 4), (2, 2), (3, 4), (4, 8))
            .AddRestaurant(anotherManager, (1, 4), (2, 2), (3, 4), (4, 8))
        );
        await Bookings.CreateBookings(b => b
            .AddBooking(users[0], restaurants[0], 1, startTime: FakeTime.GetUtcNow().AddMinutes(-20), status: BookingStatus.Seated)
            .AddBooking(users[0], restaurants[1], 2, startTime: FakeTime.GetUtcNow().AddDays(-1), status: BookingStatus.Canceled)
            .AddBooking(users[0], restaurants[0], 4, startTime: FakeTime.GetUtcNow().AddDays(1), status: BookingStatus.Pending)
            .AddBooking(users[1], restaurants[0], 3, startTime: FakeTime.GetUtcNow().AddDays(-81), status: BookingStatus.Completed)
            .AddBooking(users[1], restaurants[1], 3, startTime: FakeTime.GetUtcNow().AddDays(-11), status: BookingStatus.Completed)
            .AddBooking(users[1], restaurants[1], 2, startTime: FakeTime.GetUtcNow().AddDays(3), status: BookingStatus.Canceled)
            .AddBooking(users[2], restaurants[0], 1, startTime: FakeTime.GetUtcNow().AddDays(1), status: BookingStatus.Canceled)
            .AddBooking(users[2], restaurants[1], 1, startTime: FakeTime.GetUtcNow().AddDays(-5), status: BookingStatus.Canceled)
            .AddBooking(users[2], restaurants[0], 1, startTime: FakeTime.GetUtcNow().AddDays(2), status: BookingStatus.Pending)
        );
        NewScope();

        var res = await Mediator.Send(new GetUsersQuery()
            { RestaurantUserBeenTo = restaurants[restaurantIndex].Id.Value });
        res.Errors.Should().BeEmpty();
        res.Value.Should().HaveCount(expectedCount);
    }
}