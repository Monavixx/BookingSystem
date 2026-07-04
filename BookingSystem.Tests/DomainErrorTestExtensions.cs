using BookingSystem.Domain.Common.Errors;
using FluentAssertions.Execution;
using FluentResults;

namespace BookingSystem.Tests;

public static class DomainErrorTestExtensions
{
    public static void ShouldContain(this IResultBase result, string code)
    {
        foreach (var error in result.Errors)
        {
            if (error is DomainError de && de.Code == code)
                return;
        }

        throw new AssertionFailedException(
            $"Expected error with code '{code}' not found, the errors: " +
            string.Join(", ", result.Errors.OfType<DomainError>().Select(e => e.Code)));
    }
    public static void ShouldNotContain(this IResultBase result, string code)
    {
        foreach (var error in result.Errors)
        {
            if (error is DomainError de && de.Code == code)
                throw new AssertionFailedException($"Error happened to contain code '{code}'");
        }
    }
    public static void ShouldContain(this IResultBase result, DomainError error)
    {
        result.ShouldContain(error.Code);
    }
    public static void ShouldNotContain(this IResultBase result, DomainError error)
    {
        result.ShouldNotContain(error.Code);
    }
}