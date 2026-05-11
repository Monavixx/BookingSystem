using BookingSystem.Application.Common.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Queries;

public sealed record GetUserQuery(Guid UserId) : IRequest<Result<UserDto>>;