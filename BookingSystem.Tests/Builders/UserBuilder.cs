using BookingSystem.Domain.Users;

namespace BookingSystem.Tests.Builders;

public class UserBuilder
{
    private string _username = "monavixx";
    private string _email = "monavixx@gmail.com";
    private string _phoneNumber = "+79009009090";
    private byte[] _passwordHash = [1, 1, 1, 1];
    private DateOnly _dateOfBirth = new (1999, 10, 10);
    private string _firstName = "Danil";
    private string _lastName = "Perelygin";
    private UserRole _role = UserRole.Guest;
    private bool _isBlocked = false;
    private DateTimeOffset? _blockedUntil = null;

    public UserBuilder WithUsername(string username) { _username = username; return this; }
    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithPhoneNumber(string phoneNumber) { _phoneNumber = phoneNumber; return this; }
    public UserBuilder WithPasswordHash(byte[] passwordHash) { _passwordHash = passwordHash; return this; }
    public UserBuilder WithDateOfBirth(DateOnly dateOfBirth) { _dateOfBirth = dateOfBirth; return this; }
    public UserBuilder WithFirstName(string firstName) { _firstName = firstName; return this; }
    public UserBuilder WithLastName(string lastName) { _lastName = lastName; return this; }
    public UserBuilder WithRole(UserRole role) { _role = role; return this; }
    public UserBuilder WithIsBlocked(bool isBlocked) { _isBlocked = isBlocked; return this; }
    public UserBuilder WithBlockedUntil(DateTimeOffset? blockedUntil) { _blockedUntil = blockedUntil; return this; }

    public User Build(TimeProvider timeProvider) => 
        new User(_username, _email, _phoneNumber, _passwordHash, timeProvider.GetUtcNow(),
        _dateOfBirth, _firstName, _lastName, _role, _isBlocked, _blockedUntil);
}