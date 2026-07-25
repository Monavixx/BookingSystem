using BookingSystem.Domain.Common.Errors;
using FluentAssertions.Execution;
using FluentResults;

namespace BookingSystem.Tests;

public static class DomainErrorTestExtensions
{
    extension(IResultBase result)
    {
        public void ShouldContain(string code)
        {
            foreach (var error in result.Errors)
            {
                if (error is DomainError de && de.Code == code)
                    return;
            }

            throw new AssertionFailedException(
                $"Expected error with code '{code}' not found, the errors: " +
                string.Join(";\n", result.Errors/*.OfType<DomainError>()*/));
        }

        public void ShouldNotContain(string code)
        {
            foreach (var error in result.Errors)
            {
                if (error is DomainError de && de.Code == code)
                    throw new AssertionFailedException($"Error happened to contain code '{code}'");
            }
        }

        public void ShouldContain(DomainError error)
        {
            result.ShouldContain(error.Code);
        }

        public void ShouldNotContain(DomainError error)
        {
            result.ShouldNotContain(error.Code);
        }

        public void ShouldContain<TError>() where TError : IError
        {
            if (result.Errors.OfType<TError>().Any()) return;

            throw new AssertionFailedException(
                $"Expected error of type '{typeof(TError).Name}' not found, the errors: " +
                string.Join(", ", result.Errors.OfType<DomainError>().Select(e =>
                    $"{e.GetType().Name}:{e.Code}")));
        }

        public void ShouldNotContain<TError>() where TError : IError
        {
            if (result.Errors.OfType<TError>().Any())
                throw new AssertionFailedException($"Error happened to contain error of type '{typeof(TError).Name}'");
        }

        public void ShouldBeSuccess()
        {
            if (result.IsFailed)
                throw new AssertionFailedException(
                    $"Expected success but got errors: " +
                    $"{string.Join(", ", result.Errors.OfType<DomainError>().Select(e => e.Code))}");
        }
    }
}