namespace BookingSystem.Application.Common.DTOs;

public sealed record UserDto
(
    Guid Id,
    string Username,
    string Email,
    string PhoneNumber,
    DateTime RegistrationDateTime,
    DateOnly BirthDate,
    string FirstName,
    string LastName
);
