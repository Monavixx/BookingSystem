using BookingSystem.Api.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/users")]
public class UsersController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        //TODO:
        return Ok();
    }
}