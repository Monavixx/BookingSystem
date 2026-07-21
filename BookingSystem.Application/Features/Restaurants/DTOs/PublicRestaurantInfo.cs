using BookingSystem.Domain.Common.ValueObjects;

namespace BookingSystem.Application.Features.Restaurants.DTOs;

public sealed record PublicRestaurantInfo(
    Guid RestaurantId,
    Guid OwnerId,
    string? ImageUrl,
    string? Description,
    PublicRestaurantInfo.AddressDto? Address,
    PublicRestaurantInfo.ContactDto? Contact,
    IEnumerable<PublicRestaurantInfo.TableDto> Tables)
{
    // public static GetPublicRestaurantInfoResponse FromRestaurant(Restaurant restaurant)
    // {
    //     return new GetPublicRestaurantInfoResponse(RestaurantId: restaurant.Id.Value,
    //         ImageUrl: restaurant.ImageUrl?.Value,
    //         Description: restaurant.Description,
    //         Address: AddressDto.FromAddress(restaurant.Address),
    //         Contact: new ContactDto(PhoneNumber: restaurant.ContactPhoneNumber.Value, Email: restaurant.Email.Value),
    //         Tables: restaurant.Tables.Select(t => new TableDto(TableNumber: t.TableNumber, Capacity: t.Capacity))
    //     );
    // }

    public sealed record AddressDto(
        string Country,
        string? State,
        string? City,
        string? Street,
        string? HouseNumber,
        string? ApartmentNumber,
        string? ZipCode)
    {
        public static AddressDto FromAddress(Address address)
            => new AddressDto(Country: address.Country,
                State: address.State,
                City: address.City,
                Street: address.Street,
                HouseNumber: address.HouseNumber,
                ApartmentNumber: address.ApartmentNumber,
                ZipCode: address.ZipCode);
    }

    public sealed record ContactDto(string PhoneNumber, string Email);

    public readonly record struct TableDto(int TableNumber, int Capacity);
}