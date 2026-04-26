using BookingSystem.Domain.Common;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.Restaurant;

public sealed class Restaurant : Entity<RestaurantId>
{
    public const int DescriptionMaxLength = 350;
    private Restaurant() {}

    public Address Address { get; private set; } = null!;
    public PhoneNumber ContactPhoneNumber { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public string? Description { get; private set; }

    public Url? ImageUrl { get; private set; }
    // tables...
    
    public static Result<Restaurant> Create(
        string country,
        string? state,
        string? city,
        string? street,
        string? houseNumber,
        string? apartmentNumber,
        string? zipCode,
        string contactPhoneNumber,
        string email,
        string? description,
        string? imageUrl
        )
    {
        var addressResult = Address.Create(country, state, city, street, houseNumber, apartmentNumber, zipCode);
        var contactPhoneNumberResult = PhoneNumber.Create(contactPhoneNumber);
        var emailResult = EmailAddress.Create(email);
        Result<Url?> imageUrlResult = (imageUrl is not null ? Url.Create(imageUrl) : Result.Ok<Url?>(null)!)!;

        List<IError> errors =
        [
            ..addressResult.Errors,
            ..contactPhoneNumberResult.Errors,
            ..emailResult.Errors,
            ..imageUrlResult.Errors
        ];
        if(!string.IsNullOrWhiteSpace(description) && description.Length > DescriptionMaxLength)
            errors.Add(RestaurantErrors.Description.TooLong);
        if(errors.Count > 0)
            return Result.Fail<Restaurant>(errors);
        
        return new Restaurant
        {
            Id = RestaurantId.Create(),
            Address = addressResult.Value,
            ContactPhoneNumber = contactPhoneNumberResult.Value,
            Email = emailResult.Value,
            Description = description,
            ImageUrl = imageUrlResult.Value
        };
    }
}