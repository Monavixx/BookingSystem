using BookingSystem.Domain.Common;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.Restaurants;

public sealed class Restaurant : Entity<RestaurantId>
{
    public const int DescriptionMaxLength = 350;
    private Restaurant() {}

    public Address Address { get; private set; } = null!;
    public PhoneNumber ContactPhoneNumber { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public string? Description { get; private set; }
    public Url? ImageUrl { get; private set; }
    public WorkingSchedule? WorkingSchedule { get; private set; } = null;
    
    private readonly List<Table> _tables = [];
    public IReadOnlyCollection<Table> Tables => _tables;
    public UserId OwnerId { get; private set; }
    public Manager Owner { get; private set; } = null!;
    
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
        string? imageUrl,
        UserId ownerId,
        RestaurantId? id = null)
    {
        var addressResult = Address.Create(country, state, city, street, houseNumber, apartmentNumber, zipCode);
        var contactPhoneNumberResult = PhoneNumber.Create(contactPhoneNumber);
        var emailResult = EmailAddress.Create(email);
        var imageUrlResult = imageUrl is not null ? Url.Create(imageUrl) : Result.Ok<Url?>(null)!;
        
        var descriptionResult = (!string.IsNullOrWhiteSpace(description) && description.Length > DescriptionMaxLength)
            ? Result.Fail(RestaurantErrors.Description.TooLong)
            : Result.Ok();

        var validationResult = Result.Merge(descriptionResult, addressResult, contactPhoneNumberResult, emailResult,
            imageUrlResult);
        if (validationResult.IsFailed) return validationResult.ToResult<Restaurant>();
        
        return new Restaurant
        {
            Id = id ?? RestaurantId.Create(),
            Address = addressResult.Value,
            ContactPhoneNumber = contactPhoneNumberResult.Value,
            Email = emailResult.Value,
            Description = description,
            ImageUrl = imageUrlResult.Value,
            OwnerId = ownerId
        };
    }

    public Result AddTable(int tableNumber, int capacity)
    {
        var tableResult = Table.Create(Id, tableNumber, capacity);
        if (tableResult.IsFailed) return tableResult.ToResult();
        _tables.Add(tableResult.Value);
        return Result.Ok();
    }

    public void SetWorkingSchedule(WorkingSchedule workingSchedule)
    {
        WorkingSchedule = workingSchedule;
    }
}