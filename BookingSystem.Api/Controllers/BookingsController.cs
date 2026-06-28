using BookingSystem.Api.Common;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Features.Bookings.Commands.Confirm;
using BookingSystem.Application.Features.Bookings.Commands.ConfirmByGuest;
using BookingSystem.Application.Features.Bookings.Commands.Create;
using BookingSystem.Application.Features.Bookings.Commands.GuestSeated;
using BookingSystem.Application.Features.Bookings.Queries.Get;
using BookingSystem.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[Route("api/bookings")]
public class BookingsController(IMediator mediator) : ApiController(mediator)
{
    public record CreateBookingRequest(
        int GuestCount,
        Guid RestaurantId,
        int? TableNumber,
        DateTimeOffset ScheduledAt);
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBooking(CreateBookingRequest request)
    {
        var result = await Mediator.Send(new CreateBookingCommand(
            GuestId: User.GetUserId(),
            GuestCount: request.GuestCount, 
            RestaurantId: request.RestaurantId, 
            TableNumber: request.TableNumber, 
            ScheduledAt: request.ScheduledAt));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
    
    [HttpPost("{id:guid}/confirm-by-guest")]
    [Authorize]
    public async Task<IActionResult> ConfirmBookingByGuest([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new ConfirmBookingByGuestCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }
    
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles=$"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> ConfirmBooking([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new ConfirmBookingCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }
    
    [HttpPost("{id:guid}/guest-seated")]
    [Authorize(Roles=$"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> MarkGuestAsSeated([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new GuestSeatedCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBooking([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new GetBookingQuery(id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}