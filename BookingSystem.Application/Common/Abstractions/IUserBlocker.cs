using BookingSystem.Domain.Users.ValueObjects;
using FluentResults;

namespace BookingSystem.Application.Common.Abstractions;

public interface IUserBlocker
{
    public Task<Result> BlockUserIfCancellationPolicyViolated(UserId userId);
}