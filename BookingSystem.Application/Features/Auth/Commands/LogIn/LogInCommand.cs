using System.ComponentModel.DataAnnotations;
using BookingSystem.Application.Common.Attributes;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Auth.Commands.LogIn;

public sealed record LogInCommand : IRequest<Result<SuccessfulLogInResult>>
{
    public LogInCommand()
    {
        Password = string.Empty;
    }

    public LogInCommand(string? email, string? username, string password)
    {
        Email = email;
        Username = username;
        Password = password;
    }

    [SensitiveCommandProperty] public string? Email { get; set; }
    public string? Username { get; set; }
    [Required]
    [SensitiveCommandProperty] public string Password { get; set; }
}
