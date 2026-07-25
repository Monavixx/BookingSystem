using BookingSystem.Domain.Common.ValueObjects;
using JetBrains.Annotations;

namespace BookingSystem.Application.Common.DTOs;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record PublicRestaurantInfo(
    Guid RestaurantId,
    Guid OwnerId,
    string? ImageUrl,
    string? Description,
    PublicRestaurantInfo.AddressDto? Address,
    PublicRestaurantInfo.ContactDto? Contact,
    IEnumerable<PublicRestaurantInfo.TableDto> Tables)
{
    public PublicRestaurantInfo() :
        this(Guid.Empty, Guid.Empty, null, null, null, null, null!)
    { }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
            => new (Country: address.Country,
                State: address.State,
                City: address.City,
                Street: address.Street,
                HouseNumber: address.HouseNumber,
                ApartmentNumber: address.ApartmentNumber,
                ZipCode: address.ZipCode);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record ContactDto(string PhoneNumber, string Email);

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public readonly record struct TableDto(int TableNumber, int Capacity);
}