using BookingSystem.Api.Common;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Features.FavoriteRestaurants.Commands.AddRestaurantToFavorites;
using BookingSystem.Application.Features.FavoriteRestaurants.Commands.RemoveRestaurantFromFavorites;
using BookingSystem.Application.Features.FavoriteRestaurants.Queries.GetUsersFavoriteRestaurants;
using BookingSystem.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/restaurants")]
public class FavoriteRestaurantsController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("{id:guid}/favorite")]
    [Authorize]
    public async Task<IActionResult> AddToFavorite([FromRoute] Guid id)
    {
        var res = await Mediator.Send(new AddRestaurantToFavoritesCommand(id));
        if (res.IsFailed) return HandleErrors(res);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}/favorite")]
    [Authorize]
    public async Task<IActionResult> DeleteFromFavorite([FromRoute] Guid id)
    {
        var res = await Mediator.Send(new RemoveRestaurantFromFavoritesCommand(id));
        if (res.IsFailed) return HandleErrors(res);
        return NoContent();
    }

    [HttpGet("favorites")]
    [Authorize]
    public async Task<IActionResult> GetFavorites([FromQuery(Name = "p")] int page = 1,
        [FromQuery(Name = "ps")] int pageSize = 50)
    {
        var res = await Mediator.Send(new GetUsersFavoriteRestaurantsQuery(User.GetUserId(), page, pageSize));
        if (res.IsFailed) return HandleErrors(res);
        return Ok(res.Value);
    }

    [HttpGet("favorites/{userId:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetFavorites([FromRoute] Guid userId,
        [FromQuery(Name = "p")] int page = 1,
        [FromQuery(Name = "ps")] int pageSize = 50)
    {
        var res = await Mediator.Send(new GetUsersFavoriteRestaurantsQuery(userId, page, pageSize));
        if (res.IsFailed) return HandleErrors(res);
        return Ok(res.Value);
    }
}