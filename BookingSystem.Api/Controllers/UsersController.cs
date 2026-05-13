using BookingSystem.Api.Common;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/users")]
public class UsersController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await Mediator.Send(new GetUserQuery(HttpContext.User.GetUserId()));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}