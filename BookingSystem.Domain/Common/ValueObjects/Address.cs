using BookingSystem.Domain.Common.ValueObjects.Errors;
using FluentResults;

namespace BookingSystem.Domain.Common.ValueObjects;

public sealed record Address
{
    public const int CountryMaxLength = 100;
    public const int StateMaxLength = 100;
    public const int CityMaxLength = 100;
    public const int StreetMaxLength = 200;
    public const int HouseNumberMaxLength = 20;
    public const int ApartmentNumberMaxLength = 20;
    public const int ZipCodeMaxLength = 20;
    private Address()
    { }

    public static Result<Address> Create(
        string country,
        string? state,
        string? city,
        string? street,
        string? houseNumber,
        string? apartmentNumber,
        string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Fail<Address>(AddressErrors.Country.Empty);
        
        return new Address()
        {
            Country = country,
            State = state,
            City = city,
            Street = street,
            HouseNumber = houseNumber,
            ApartmentNumber = apartmentNumber,
            ZipCode = zipCode
        };
    }

    public string Country { get; private init; } = null!;
    public string? State { get; private init; }
    public string? City { get; private init; }
    public string? Street { get; private init; }
    public string? HouseNumber { get; private init; }
    public string? ApartmentNumber { get; private init; }
    public string? ZipCode { get; private init; }

    public void Deconstruct(out string country, out string? state, out string? city, out string? street,
        out string? houseNumber, out string? apartmentNumber, out string? zipCode)
    {
        country = this.Country;
        state = this.State;
        city = this.City;
        street = this.Street;
        houseNumber = this.HouseNumber;
        apartmentNumber = this.ApartmentNumber;
        zipCode = this.ZipCode;
    }
}