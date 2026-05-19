using BookingSystem.Application.Common.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Queries;

public sealed record GetUserQuery : IRequest<Result<UserDto>>
{
    public static readonly GetUserQuery Default = new(); 
}