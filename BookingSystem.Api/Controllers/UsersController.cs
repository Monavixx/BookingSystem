using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Users.Commands.Block;
using BookingSystem.Application.Features.Users.Commands.MakeManager;
using BookingSystem.Application.Features.Users.Queries.GetCurrentUser;
using BookingSystem.Application.Features.Users.Queries.GetUsers;
using BookingSystem.Domain.Users;
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

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    public async Task<IActionResult> GetUsers(GetUsersRequest request)
    {
        var query = request.ToQuery();
        var result = await Mediator.Send(query);
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
    public record GetUsersRequest(
        [FromQuery(Name = "rubt")] Guid? RestaurantUserBeenTo,
        [FromQuery(Name = "ruia")] Guid? RestaurantUserIsAt,
        [FromQuery(Name = "ot")] int? OlderThan,
        [FromQuery(Name = "yt")] int? YoungerThan,
        [FromQuery(Name = "bcgt")] int? BookingCountGreaterThan,
        [FromQuery(Name = "bclt")] int? BookingCountLessThan,
        [FromQuery(Name = "blocked")] bool? IsBlocked,
        [FromQuery(Name = "p")] int Page = 1,
        [FromQuery(Name = "ps")] int PageSize = 50
    )
    {
        public GetUsersQuery ToQuery() => new(
            RestaurantUserBeenTo: RestaurantUserBeenTo,
            RestaurantUserIsAt: RestaurantUserIsAt,
            OlderThan: OlderThan,
            YoungerThan: YoungerThan,
            BookingCountGreaterThan: BookingCountGreaterThan,
            BookingCountLessThan: BookingCountLessThan,
            IsBlocked: IsBlocked,
            Page: Page,
            PageSize: PageSize
        );
    }
}