using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Restaurants.Commands.AddTableToRestaurant;
using BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;
using BookingSystem.Application.Features.Restaurants.Queries.GetPublicRestaurantInfo;
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
        return Ok(result.Value);
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
}