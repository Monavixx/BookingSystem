using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/users")]
public class UsersController(IMediator mediator) : ApiController(mediator)
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await Mediator.Send(GetUserQuery.Default);
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}