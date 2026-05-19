using BookingSystem.Application.Common.Attributes;
using BookingSystem.Application.Common.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshCommand ([property:SensitiveCommandProperty] string RefreshToken) : IRequest<Result<AuthTokens>>;