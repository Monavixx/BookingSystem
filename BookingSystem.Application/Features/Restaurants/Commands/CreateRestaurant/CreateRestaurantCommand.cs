using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;

public sealed record CreateRestaurantCommand(
    CreateRestaurantCommand.AddressDto Address,
    CreateRestaurantCommand.ContactDto Contact,
    string? Description,
    string? ImageUrl)
    : IRequest<Result<CreateRestaurantResult>>
{
    public sealed record AddressDto(
        string Country,
        string? State,
        string? City,
        string? Street,
        string? HouseNumber,
        string? ApartmentNumber,
        string? ZipCode);

    public sealed record ContactDto(string PhoneNumber, string Email);

}