using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Common.ValueObjects.Errors;

public static class AddressErrors
{
    public static class Country
    {
        public static readonly ValidationError Empty = new ("Address.Country.Empty", "Country cannot be empty");
    }
}