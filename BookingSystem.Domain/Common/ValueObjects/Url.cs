using BookingSystem.Domain.Common.ValueObjects.Errors;
using FluentResults;

namespace BookingSystem.Domain.Common.ValueObjects;

public sealed record Url
{
    public const int MaxLength = 2048;
    private Url() { }

    public static Result<Url> Create(string url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            return Result.Fail<Url>(UrlErrors.InvalidFormat);
        return new Url { Value = url };
    }
    public string Value { get; private init; } = null!;

    public void Deconstruct(out string url)
    {
        url = Value;
    }
}