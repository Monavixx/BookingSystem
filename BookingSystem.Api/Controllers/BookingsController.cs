using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Bookings.Commands.Cancel;
using BookingSystem.Application.Features.Bookings.Commands.Confirm;
using BookingSystem.Application.Features.Bookings.Commands.ConfirmByGuest;
using BookingSystem.Application.Features.Bookings.Commands.Create;
using BookingSystem.Application.Features.Bookings.Commands.GuestSeated;
using BookingSystem.Application.Features.Bookings.Queries.Get;
using BookingSystem.Application.Features.Bookings.Queries.GetAll;
using BookingSystem.Domain.Bookings.ValueObjects;
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
            GuestCount: request.GuestCount,
            RestaurantId: request.RestaurantId,
            TableNumber: request.TableNumber,
            ScheduledAt: request.ScheduledAt));
        if (result.IsFailed) return HandleErrors(result);
        return CreatedAtAction(nameof(GetBooking), new { id = result.Value.Id }, result.Value);
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
    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> ConfirmBooking([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new ConfirmBookingCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    [HttpPost("{id:guid}/guest-seated")]
    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> MarkGuestAsSeated([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new GuestSeatedCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(
        [FromRoute] Guid id,
        [FromQuery(Name = "guestAsked")] bool isGuestRequest = false)
    {
        var result = await Mediator.Send(
            new CancelBookingCommand(BookingId: id,
                IsGuestRequest: isGuestRequest));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    public async Task<IActionResult> CompleteBooking([FromRoute] Guid id)
    {
        var result = await Mediator.Send(
            new CompleteBookingByManagerCommand(BookingId: id));
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

    public sealed record GetBookingsRequest(
        [FromQuery(Name = "rid")] Guid? RestaurantId = null,
        [FromQuery(Name = "tn")] int? TableNumber = null,
        [FromQuery(Name = "s")] BookingStatus? Status = null,
        [FromQuery(Name = "start")] DateTimeOffset? Start = null,
        [FromQuery(Name = "end")] DateTimeOffset? End = null,
        [FromQuery(Name = "tfm")] TimeFilterMethod TimeFilterMethod = TimeFilterMethod.In,
        [FromQuery(Name = "gid")] Guid? GuestId = null,
        [FromQuery(Name = "mode")] FilterMode FilterMode = FilterMode.All,
        [FromQuery(Name = "p")] int Page = 1,
        [FromQuery(Name = "ps")] int PageSize = 50)
    {
        public GetAllBookingsQuery ToQuery() => new(
            RestaurantId: RestaurantId,
            TableNumber: TableNumber,
            Status: Status,
            Start: Start,
            End: End,
            TimeFilterMethod: TimeFilterMethod,
            GuestId: GuestId,
            FilterMode: FilterMode,
            Page: Page,
            PageSize: PageSize);
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetBookings([FromQuery] GetBookingsRequest query)
    {
        var result = await Mediator.Send(query.ToQuery());
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}
