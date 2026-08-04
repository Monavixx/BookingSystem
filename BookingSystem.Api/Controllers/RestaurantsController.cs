using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Restaurants.Commands.AddTable;
using BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;
using BookingSystem.Application.Features.Restaurants.Commands.DeleteRestaurant;
using BookingSystem.Application.Features.Restaurants.Commands.DeleteTable;
using BookingSystem.Application.Features.Restaurants.Commands.SetWorkingSchedule;
using BookingSystem.Application.Features.Restaurants.Commands.UpdateRestaurant;
using BookingSystem.Application.Features.Restaurants.DTOs;
using BookingSystem.Application.Features.Restaurants.Queries.GetPublicRestaurantInfo;
using BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantList;
using BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantWorkingSchedule;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

/// <summary>
/// Provides endpoints for managing restaurants, including creation, deletion, updates, and retrieval of restaurant information.
/// </summary>
[Route("api/restaurants")]
public class RestaurantsController(IMediator mediator) : ApiController(mediator)
{
    /// <summary>
    /// Creates a new restaurant.
    /// </summary>
    /// <param name="command">The restaurant creation command containing name, address, contact, and other details.</param>
    /// <returns>The created restaurant with 201 status, or error response.</returns>
    /// <response code="201">Restaurant created successfully.</response>
    /// <response code="400">Invalid request data or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Manager role.</response>
    /// <response code="409">Restaurant already exists or conflict with existing data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailed) return HandleErrors(result);
        return CreatedAtAction(
            nameof(GetPublicRestaurantInfo),
            new { restaurantId = result.Value.Id },
            result.Value);
    }

    public record AddTableRequestBody(int TableNumber, int Capacity);

    /// <summary>
    /// Adds a table to a specific restaurant.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <param name="request">The request body containing table number and seating capacity.</param>
    /// <returns>No content on success, or error response.</returns>
    /// <response code="204">Table added successfully.</response>
    /// <response code="400">Invalid request data or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Manager role.</response>
    /// <response code="404">Restaurant not found.</response>
    /// <response code="409">Table with this number already exists for the restaurant.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{restaurantId:guid}/add-table")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddTable([FromRoute] Guid restaurantId,
        [FromBody] AddTableRequestBody request)
    {
        var result =
            await Mediator.Send(new AddTableCommand(restaurantId, request.TableNumber, request.Capacity));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    /// <summary>
    /// Deletes multiple tables from a restaurant.
    /// </summary>
    /// <param name="tableIds">Collection of table identifiers to delete.</param>
    /// <returns>No content on success, or error response.</returns>
    /// <response code="204">Tables deleted successfully.</response>
    /// <response code="400">Invalid request data or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Manager role.</response>
    /// <response code="404">One or more tables not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("delete-tables")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTables([FromBody] ICollection<TableId> tableIds)
    {
        var result = await Mediator.Send(new DeleteTablesCommand(tableIds));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    /// <summary>
    /// Retrieves public information about a specific restaurant.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <returns>Public restaurant information, or error response.</returns>
    /// <response code="200">Restaurant information retrieved successfully.</response>
    /// <response code="400">Invalid restaurant ID format.</response>
    /// <response code="404">Restaurant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{restaurantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPublicRestaurantInfo(Guid restaurantId)
    {
        var result = await Mediator.Send(new GetPublicRestaurantInfoQuery(restaurantId));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves a paginated list of all restaurants, optionally filtered by city.
    /// </summary>
    /// <param name="page">The page number (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of restaurants per page. Defaults to 50.</param>
    /// <param name="city">Optional city filter to retrieve restaurants in a specific city.</param>
    /// <returns>Paginated list of restaurants, or error response.</returns>
    /// <response code="200">Restaurant list retrieved successfully.</response>
    /// <response code="400">Invalid pagination parameters or query filters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRestaurants(
        [FromQuery(Name = "p")] int page = 1,
        [FromQuery(Name = "ps")] int pageSize = 50,
        [FromQuery(Name = "city")] string? city = null)
    {
        var result = await Mediator.Send(new GetRestaurantListQuery(page, pageSize, city));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a restaurant.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant to delete.</param>
    /// <returns>No content on success, or error response.</returns>
    /// <response code="204">Restaurant deleted successfully.</response>
    /// <response code="400">Invalid restaurant ID format.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Manager role.</response>
    /// <response code="404">Restaurant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{restaurantId:guid}")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRestaurant([FromRoute] Guid restaurantId)
    {
        var result = await Mediator.Send(new DeleteRestaurantCommand(restaurantId));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    /// <summary>
    /// Updates restaurant information including address, contact details, description, and image.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant to update.</param>
    /// <param name="request">The request body containing updated address, contact, description, and image URL.</param>
    /// <returns>No content on success, or error response.</returns>
    /// <response code="204">Restaurant updated successfully.</response>
    /// <response code="400">Invalid request data or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Manager role.</response>
    /// <response code="404">Restaurant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{restaurantId:guid}")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRestaurant([FromRoute] Guid restaurantId,
        [FromBody] UpdateRestaurantRequestBody request)
    {
        var result = await Mediator.Send(new UpdateRestaurantCommand(restaurantId, request.Address, request.Contact,
            request.Description, request.ImageUrl));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    public record UpdateRestaurantRequestBody(
        UpdateRestaurantCommand.AddressDto Address,
        UpdateRestaurantCommand.ContactDto Contact,
        string? Description,
        string? ImageUrl);

    /// <summary>
    /// Sets the working schedule for a restaurant, specifying which days and hours the restaurant operates.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <param name="request">The request body containing working days and their schedule information.</param>
    /// <returns>No content on success, or error response.</returns>
    /// <response code="204">Working schedule set successfully.</response>
    /// <response code="400">Invalid request data or validation failure.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Manager role.</response>
    /// <response code="404">Restaurant not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{restaurantId:guid}/set-working-schedule")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetWorkingSchedule([FromRoute] Guid restaurantId,
        [FromBody] SetWorkingScheduleRequestBody request)
    {
        var result = await Mediator.Send(new SetWorkingScheduleCommand(restaurantId, request.WorkingDays));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    public record SetWorkingScheduleRequestBody(IEnumerable<DayOfWeekScheduleRequest> WorkingDays);

    /// <summary>
    /// Retrieves the working schedule of a specific restaurant.
    /// </summary>
    /// <param name="restaurantId">The unique identifier of the restaurant.</param>
    /// <returns>Restaurant working schedule, or error response.</returns>
    /// <response code="200">Working schedule retrieved successfully.</response>
    /// <response code="400">Invalid restaurant ID format.</response>
    /// <response code="404">Restaurant not found or has no working schedule defined.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{restaurantId:guid}/working-schedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkingSchedule([FromRoute] Guid restaurantId)
    {
        var result = await Mediator.Send(new GetRestaurantWorkingScheduleQuery(restaurantId));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}
