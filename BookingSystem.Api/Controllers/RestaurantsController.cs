using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Restaurants.Commands.AddTableToRestaurant;
using BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;
using BookingSystem.Application.Features.Restaurants.Commands.DeleteRestaurant;
using BookingSystem.Application.Features.Restaurants.Commands.SetWorkingSchedule;
using BookingSystem.Application.Features.Restaurants.Commands.UpdateRestaurant;
using BookingSystem.Application.Features.Restaurants.DTOs;
using BookingSystem.Application.Features.Restaurants.Queries.GetPublicRestaurantInfo;
using BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantList;
using BookingSystem.Domain.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/restaurants")]
public class RestaurantsController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantCommand command)
    {
        var result = await Mediator.Send(command);
        if (result.IsFailed) return HandleErrors(result);
        return CreatedAtAction(
            nameof(GetPublicRestaurantInfo),
            new { restaurantId = result.Value.Id },
            result.Value);
    }

    public record AddTableToRestaurantRequestBody(int TableNumber, int Capacity);

    [HttpPost("{restaurantId:guid}/add-table")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public async Task<IActionResult> AddTableToRestaurant([FromRoute] Guid restaurantId,
        [FromBody] AddTableToRestaurantRequestBody request)
    {
        var result =
            await Mediator.Send(new AddTableToRestaurantCommand(restaurantId, request.TableNumber, request.Capacity));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }
    
    [HttpGet("{restaurantId:guid}")]
    public async Task<IActionResult> GetPublicRestaurantInfo(Guid restaurantId)
    {
        var result = await Mediator.Send(new GetPublicRestaurantInfoQuery(restaurantId));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetRestaurants(
        [FromQuery(Name = "lt")] int limit = 10,
        [FromQuery(Name = "sp")] int skip = 0,
        [FromQuery(Name="city")] string? city = null)
    {
        var result = await Mediator.Send(new GetRestaurantListQuery(limit, skip, city));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }

    [HttpDelete("{restaurantId:guid}")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public async Task<IActionResult> DeleteRestaurant([FromRoute] Guid restaurantId)
    {
        var result = await Mediator.Send(new DeleteRestaurantCommand(restaurantId));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    [HttpPut("{restaurantId:guid}")]
    [Authorize(Roles = nameof(UserRole.Manager))]
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

    [HttpPost("{restaurantId:guid}/set-working-schedule")]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public async Task<IActionResult> SetWorkingSchedule([FromRoute] Guid restaurantId,
        [FromBody] SetWorkingScheduleRequestBody request)
    {
        var result = await Mediator.Send(new SetWorkingScheduleCommand(restaurantId, request.WorkingDays));
        if (result.IsFailed) return HandleErrors(result);
        return NoContent();
    }

    public record SetWorkingScheduleRequestBody(IEnumerable<DayOfWeekScheduleDto> WorkingDays);
}