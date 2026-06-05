using BookingSystem.Api.Common;
using BookingSystem.Api.Extensions;
using BookingSystem.Application.Features.Bookings.Commands.ConfirmBookingByGuest;
using BookingSystem.Application.Features.Bookings.Commands.CreateBooking;
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
}