using BookingSystem.Api.Common;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Features.Users.Commands.Block;
using BookingSystem.Application.Features.Users.Commands.MakeManager;
using BookingSystem.Application.Features.Users.DTOs;
using BookingSystem.Application.Features.Users.Queries.GetCurrentUser;
using BookingSystem.Application.Features.Users.Queries.GetUsers;
using BookingSystem.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

/// <summary>
/// Manages user-related operations including retrieving user information, promoting users to managers, and blocking users.
/// </summary>
[Route("api/users")]
public class UsersController(IMediator mediator) : ApiController(mediator)
{
    /// <summary>
    /// Retrieves the current authenticated user's information.
    /// </summary>
    /// <remarks>
    /// Requires authentication. Returns the profile information of the currently logged-in user.
    /// </remarks>
    /// <response code="200">Successfully retrieved the current user's profile.</response>
    /// <response code="401">User is not authenticated or token is invalid.</response>
    /// <response code="404">The current user was not found in the system.</response>
    [Authorize]
    [HttpGet("me")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await Mediator.Send(GetCurrentUserQuery.Default);
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }

    /// <summary>
    /// Promotes a user to the manager role.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be promoted to manager.</param>
    /// <remarks>
    /// Requires administrator privileges. Elevates the specified user's role from Guest to Manager,
    /// allowing them to manage restaurant operations.
    /// </remarks>
    /// <response code="204">The user has been successfully promoted to manager.</response>
    /// <response code="403">User lacks administrator privileges to perform this action.</response>
    /// <response code="404">The specified user was not found in the system.</response>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{userId:guid}/make-manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MakeManager([FromRoute] Guid userId)
    {
        var result = await Mediator.Send(new MakeManagerCommand(userId));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    /// <summary>
    /// Blocks a user from using the platform.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to be blocked.</param>
    /// <param name="duration">Optional. The duration for which the user should be blocked. If not provided, the block is permanent.</param>
    /// <remarks>
    /// Requires administrator privileges. Prevents a user from performing booking-related actions.
    /// The block can be temporary (with a specified duration) or permanent (when no duration is provided).
    /// Admin users cannot be blocked.
    /// </remarks>
    /// <response code="204">The user has been successfully blocked.</response>
    /// <response code="403">User lacks administrator privileges, or the target user is an admin and cannot be blocked.</response>
    /// <response code="404">The specified user was not found in the system.</response>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{userId:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockUser([FromRoute] Guid userId, [FromBody] TimeSpan? duration = null)
    {
        var result = await Mediator.Send(new BlockUserCommand(userId, duration));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional filtering criteria.
    /// </summary>
    /// <param name="request">Query parameters for filtering and pagination.</param>
    /// <remarks>
    /// Requires administrator privileges. Returns a filtered list of users based on various criteria including
    /// age range, booking count, block status, and restaurant history. The results are paginated and exclude the current user.
    /// </remarks>
    /// <response code="200">Successfully retrieved the list of users matching the filter criteria.</response>
    /// <response code="403">User lacks administrator privileges to access this resource.</response>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
        /// <summary>
        /// Converts the query request to a GetUsersQuery command.
        /// </summary>
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
