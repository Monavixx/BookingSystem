using BookingSystem.Application.Common.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public static readonly GetCurrentUserQuery Default = new(); 
}