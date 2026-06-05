using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Domain.Users.Errors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserHandler(ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await currentUserService.GetUserAsync();
        if (user is null) return Result.Fail<UserDto>(UserErrors.NotFound);
        
        return Result.Ok(UserDto.FromUser(user));
    }
}