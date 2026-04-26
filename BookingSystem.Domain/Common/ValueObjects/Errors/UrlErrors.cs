using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Domain.Common.ValueObjects.Errors;

public static class UrlErrors
{
    public static readonly ValidationError InvalidFormat = new("Url.InvalidFormat", "Invalid URL format");
}