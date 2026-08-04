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

/// <summary>
/// Manages user favorite restaurants.
/// </summary>
[Route("api/restaurants")]
public class FavoriteRestaurantsController(IMediator mediator) : ApiController(mediator)
{
    /// <summary>
    /// Adds a restaurant to the current user's favorites.
    /// </summary>
    /// <param name="id">The restaurant ID to add to favorites.</param>
    /// <returns>No content if the restaurant was successfully added to favorites.</returns>
    /// <response code="204">Restaurant successfully added to favorites.</response>
    /// <response code="400">Validation error or reference error. Details include error codes and messages.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id:guid}/favorite")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToFavorite([FromRoute] Guid id)
    {
        var res = await Mediator.Send(new AddRestaurantToFavoritesCommand(id));
        if (res.IsFailed) return HandleErrors(res);
        return NoContent();
    }
    
    /// <summary>
    /// Removes a restaurant from the current user's favorites.
    /// </summary>
    /// <param name="id">The restaurant ID to remove from favorites.</param>
    /// <returns>No content if the restaurant was successfully removed from favorites.</returns>
    /// <response code="204">Restaurant successfully removed from favorites.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="404">Not found - the restaurant is not in the user's favorites.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}/favorite")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFromFavorite([FromRoute] Guid id)
    {
        var res = await Mediator.Send(new RemoveRestaurantFromFavoritesCommand(id));
        if (res.IsFailed) return HandleErrors(res);
        return NoContent();
    }

    /// <summary>
    /// Gets the current user's favorite restaurants with pagination.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 50).</param>
    /// <returns>A list of the user's favorite restaurants.</returns>
    /// <response code="200">Successfully retrieved the user's favorite restaurants.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("favorites")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFavorites([FromQuery(Name = "p")] int page = 1,
        [FromQuery(Name = "ps")] int pageSize = 50)
    {
        var res = await Mediator.Send(new GetUsersFavoriteRestaurantsQuery(User.GetUserId(), page, pageSize));
        if (res.IsFailed) return HandleErrors(res);
        return Ok(res.Value);
    }

    /// <summary>
    /// Gets a specific user's favorite restaurants. Requires admin role.
    /// </summary>
    /// <param name="userId">The user ID to retrieve favorites for.</param>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 50).</param>
    /// <returns>A list of the specified user's favorite restaurants.</returns>
    /// <response code="200">Successfully retrieved the user's favorite restaurants.</response>
    /// <response code="401">Unauthorized - user is not authenticated.</response>
    /// <response code="403">Forbidden - user does not have admin role.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("favorites/{userId:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFavorites([FromRoute] Guid userId,
        [FromQuery(Name = "p")] int page = 1,
        [FromQuery(Name = "ps")] int pageSize = 50)
    {
        var res = await Mediator.Send(new GetUsersFavoriteRestaurantsQuery(userId, page, pageSize));
        if (res.IsFailed) return HandleErrors(res);
        return Ok(res.Value);
    }
}