using BookingSystem.Application.Features.Bookings.Queries.GetAll;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Tests.Services;
using FluentAssertions;

namespace BookingSystem.Tests.Application.Features.Bookings.Queries;

public class GetAllBookingsHandlerTests(PostgresTestFixture dbFixture) : IntegrationTestBase(dbFixture)
{
    private Base5Users _users = null!;
    private Restaurant _restaurant1 = null!, _restaurant2 = null!;
    private Booking[] _bookings = null!;

    protected override async Task InitAsync()
    {
        _users = await Users.CreateBase5Async();
        var restaurants = await Restaurants.CreateRestaurants(builder => builder
            .AddRestaurant(_users.Manager, (1, 4), (2, 2))
            .AddRestaurant(_users.AnotherManager, (1, 3), (2, 8), (3, 10), (4, 8), (5, 2))
        );
        _restaurant1 = restaurants[0];
        _restaurant2 = restaurants[1];

        _bookings = await Bookings.CreateBookings(config => config
            .AddBooking(_users.Guest, _restaurant1, 1, guestCount: 2, status: BookingStatus.Pending,
                startTime: FakeTime.GetUtcNow().AddHours(1))
            .AddBooking(_users.Guest, _restaurant1, 2, guestCount: 2, status: BookingStatus.Confirmed,
                startTime: FakeTime.GetUtcNow().AddHours(2))
            .AddBooking(_users.AnotherGuest, _restaurant1, 1, guestCount: 4, status: BookingStatus.Canceled,
                startTime: FakeTime.GetUtcNow().AddHours(3))
            .AddBooking(_users.AnotherGuest, _restaurant2, 3, guestCount: 6, status: BookingStatus.Confirmed,
                startTime: FakeTime.GetUtcNow().AddHours(4))
            .AddBooking(_users.AnotherManager, _restaurant1, 2, guestCount: 1,
                startTime: FakeTime.GetUtcNow().AddHours(5))
        );
        NewScope();
    }

    [Fact]
    public async Task When_Admin_NoFilterProvided_ShouldReturnAllBookings()
    {
        SetCurrentUser(_users.Admin);
        var res = await Mediator.Send(new GetAllBookingsQuery());

        res.IsSuccess.Should().BeTrue();
        res.Value.Count.Should().Be(_bookings.Length);
        res.Value.Select(x => x.Id).Should().BeEquivalentTo(_bookings.Select(b => b.Id.Value));
    }

    [Fact]
    public async Task When_Admin_RestaurantFilterProvided_ShouldReturnAllBookingsFromTheRestaurant()
    {
        SetCurrentUser(_users.Admin);
        var res = await Mediator.Send(new GetAllBookingsQuery(RestaurantId: _restaurant1.Id.Value));

        res.IsSuccess.Should().BeTrue();
        res.Value.Count.Should().Be(4);
    }

    [Fact]
    public async Task When_Manager_NoFilterProvided_ShouldReturnAllTheirBookingsAndBookingsFromRestaurantsOwnedByThem()
    {
        SetCurrentUser(_users.AnotherManager);
        var res = await Mediator.Send(new GetAllBookingsQuery());
        res.IsSuccess.Should().BeTrue();
        res.Value.Count.Should().Be(2);
    }

    [Fact]
    public async Task When_Guest_NoFilterProvided_ShouldReturnAllTheirBookings()
    {
        SetCurrentUser(_users.Guest);
        var res = await Mediator.Send(new GetAllBookingsQuery());
        res.IsSuccess.Should().BeTrue();
        res.Value.Count.Should().Be(2);
    }

    [Fact]
    public async Task When_Guest_FiltersProvided_SuchBookingsDoesNotExist_ShouldReturnEmptyCollection()
    {
        SetCurrentUser(_users.Guest);
        var res = await Mediator.Send(new GetAllBookingsQuery(TableNumber: 10));
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task When_Admin_StartFilterProvided_ShouldReturnAllBookingsThatStartsNotEarlierThanRequested()
    {
        var start = FakeTime.GetUtcNow().AddHours(3.5);
        SetCurrentUser(_users.Admin);
        var res = await Mediator.Send(new GetAllBookingsQuery(Start: start, TimeFilterMethod: TimeFilterMethod.In));
        res.IsSuccess.Should().BeTrue();
        res.Value.Count.Should().Be(2);
        res.Value.Should().OnlyContain(b => b.Start >= start);
    }
}