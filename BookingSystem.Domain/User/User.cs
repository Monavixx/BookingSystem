using BookingSystem.Domain.Common;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Common.ValueObjects.Errors;
using BookingSystem.Domain.User.Errors;
using BookingSystem.Domain.User.ValueObjects;
using FluentResults;

namespace BookingSystem.Domain.User;

using FavoriteRestaurant = BookingSystem.Domain.FavoriteRestaurant.FavoriteRestaurant;

public sealed class User : Entity<UserId>
{
    public const int FirstNameMaxLength = 100;
    public const int LastNameMaxLength = 150;
    public const int PasswordHashMaxLength = 150;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 150;
    private User()
    { }

    public Username Username { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public byte[] PasswordHash { get; private set; } = null!;
    public RegistrationDateTime RegistrationDateTime { get; private set; }
    public Birthdate BirthDate { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public UserRole Role { get; private set; } = UserRole.Guest;

    private readonly List<Session> _sessions = [];
    public IReadOnlyCollection<Session> Sessions => _sessions;

    private readonly List<FavoriteRestaurant> _favoriteRestaurants = [];
    public IReadOnlyCollection<FavoriteRestaurant> FavoriteRestaurants => _favoriteRestaurants;


    public static Result<User> Create(
        string username,
        string email,
        string phoneNumber,
        byte[]? passwordHash,
        DateOnly birthdate,
        string firstName,
        string lastName,
        UserRole role = UserRole.Guest)
    {
        var usernameResult = Username.Create(username);
        var emailResult = EmailAddress.Create(email);
        var phoneNumberResult = PhoneNumber.Create(phoneNumber);
        var birthdateResult = Birthdate.Create(birthdate);
        if (!Enum.IsDefined(role))
            throw new ArgumentException("Invalid user role", nameof(role));
        List<IError> errors =
        [
            ..usernameResult.Errors,
            ..emailResult.Errors,
            ..phoneNumberResult.Errors,
            ..birthdateResult.Errors
        ];
        if(string.IsNullOrWhiteSpace(firstName)) errors.Add(UserErrors.FirstName.Empty);
        if(firstName.Length > FirstNameMaxLength) errors.Add(UserErrors.FirstName.TooLong);
        if(string.IsNullOrWhiteSpace(lastName)) errors.Add(UserErrors.LastName.Empty);
        if(lastName.Length > LastNameMaxLength) errors.Add(UserErrors.LastName.TooLong);
        if(passwordHash is null || passwordHash.Length == 0) errors.Add(UserErrors.PasswordHash.Empty);

        if (errors.Count > 0)
            return Result.Fail<User>(errors).MapErrors(e =>
                e switch
                {
                    _ when e == EmailAddressErrors.Empty => UserErrors.Email.Empty,
                    _ when e == EmailAddressErrors.InvalidFormat => UserErrors.Email.InvalidFormat,
                    _ when e == EmailAddressErrors.TooLong => UserErrors.Email.TooLong,
                    _ when e == PhoneNumberErrors.Empty => UserErrors.PhoneNumber.Empty,
                    _ when e == PhoneNumberErrors.InvalidFormat => UserErrors.PhoneNumber.InvalidFormat,
                    _ when e == PhoneNumberErrors.TooLong => UserErrors.PhoneNumber.TooLong,
                    _ => e
                });
        
        return new User
        {
            Id = UserId.New(),
            Username = usernameResult.Value,
            Email = emailResult.Value,
            PhoneNumber = phoneNumberResult.Value,
            PasswordHash = passwordHash!,
            RegistrationDateTime = RegistrationDateTime.New(),
            BirthDate = birthdateResult.Value,
            FirstName = firstName,
            LastName = lastName,
            Role = role
        };
    }

    public void AddSession(RefreshToken refreshToken)
    {
        _sessions.Add(Session.Create(Id, refreshToken));
    }
}