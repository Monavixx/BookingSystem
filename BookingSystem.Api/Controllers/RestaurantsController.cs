using BookingSystem.Api.Common;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;
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
        if(result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}