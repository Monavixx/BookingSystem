using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Users.Commands.SignUp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/auth")]
public class AuthController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(UsersController.GetCurrentUser),
                nameof(UsersController).Replace("Controller", ""), null, result.Value);
        return HandleErrors(result);
    }
}