using BookingSystem.Domain.User.ValueObjects;

namespace BookingSystem.Application.Common.DTOs;

public sealed record AuthTokens
(
    string AccessToken,
    RefreshToken RefreshToken
);