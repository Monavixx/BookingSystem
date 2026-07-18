using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.UpdateRestaurant;

public sealed record UpdateRestaurantCommand(
    Guid RestaurantId,
    UpdateRestaurantCommand.AddressDto Address,
    UpdateRestaurantCommand.ContactDto Contact,
    string? Description,
    string? ImageUrl)
    : IRequest<Result>, IRequireActiveUser
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