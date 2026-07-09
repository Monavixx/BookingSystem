using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Users.Commands.Block;
using BookingSystem.Application.Features.Users.Commands.MakeManager;
using BookingSystem.Application.Features.Users.Queries.GetCurrentUser;
using BookingSystem.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BookingSystem.Api.Controllers;

[Route("api/users")]
public class UsersController(IMediator mediator) : ApiController(mediator)
{
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await Mediator.Send(GetCurrentUserQuery.Default);
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{userId:guid}/make-manager")]
    public async Task<IActionResult> MakeManager([FromRoute] Guid userId)
    {
        var result = await Mediator.Send(new MakeManagerCommand(userId));
        if(result.IsFailed) return HandleErrors(result);
        return NoContent();
    }
    
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{userId:guid}/block")]
    public async Task<IActionResult> BlockUser([FromRoute] Guid userId, [FromBody] TimeSpan? duration = null)
    {
        var result = await Mediator.Send(new BlockUserCommand(userId, duration));
        if(result.IsFailed) return HandleErrors(result);
        return NoContent();
    }
}