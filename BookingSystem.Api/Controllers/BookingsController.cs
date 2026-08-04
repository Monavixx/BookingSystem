using BookingSystem.Api.Common;
using BookingSystem.Application.Features.Bookings.Commands.Cancel;
using BookingSystem.Application.Features.Bookings.Commands.Complete;
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

/// <summary>
/// Manages booking operations including creation, confirmation, cancellation, and status updates.
/// </summary>
[Route("api/bookings")]
public class BookingsController(IMediator mediator) : ApiController(mediator)
{
    public record CreateBookingRequest(
        int GuestCount,
        Guid RestaurantId,
        int? TableNumber,
        DateTimeOffset ScheduledAt);

    /// <summary>
    /// Creates a new booking for a guest at a restaurant.
    /// </summary>
    /// <param name="request">The booking creation request containing guest count, restaurant ID, optional table number, and scheduled time.</param>
    /// <returns>The newly created booking with HTTP 201 Created status.</returns>
    /// <response code="201">Booking created successfully.</response>
    /// <response code="400">Invalid request data or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Restaurant or table not found.</response>
    /// <response code="409">Booking conflict (e.g., table already booked for that time).</response>
    /// <response code="422">Unprocessable request entity.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Confirms a booking by the guest.
    /// </summary>
    /// <param name="id">The booking ID to confirm.</param>
    /// <returns>HTTP 200 OK on successful confirmation.</returns>
    /// <response code="200">Booking confirmed successfully by guest.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Booking not found.</response>
    /// <response code="409">Booking cannot be confirmed due to conflict with current state.</response>
    /// <response code="422">Unprocessable request entity.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id:guid}/confirm-by-guest")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmBookingByGuest([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new ConfirmBookingByGuestCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    /// <summary>
    /// Confirms a booking by a manager or admin.
    /// </summary>
    /// <param name="id">The booking ID to confirm.</param>
    /// <returns>HTTP 200 OK on successful confirmation.</returns>
    /// <response code="200">Booking confirmed successfully by manager/admin.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have the required Manager or Admin role.</response>
    /// <response code="404">Booking not found.</response>
    /// <response code="409">Booking cannot be confirmed due to conflict with current state.</response>
    /// <response code="422">Unprocessable request entity.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmBooking([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new ConfirmBookingCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    /// <summary>
    /// Marks a guest as seated for a confirmed booking.
    /// </summary>
    /// <param name="id">The booking ID to mark the guest as seated.</param>
    /// <returns>HTTP 200 OK on successful update.</returns>
    /// <response code="200">Guest marked as seated successfully.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have the required Manager or Admin role.</response>
    /// <response code="404">Booking not found.</response>
    /// <response code="409">Booking cannot be updated due to conflict with current state.</response>
    /// <response code="422">Unprocessable request entity.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id:guid}/guest-seated")]
    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkGuestAsSeated([FromRoute] Guid id)
    {
        var result = await Mediator.Send(new GuestSeatedCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    /// <summary>
    /// Cancels a booking.
    /// </summary>
    /// <param name="id">The booking ID to cancel.</param>
    /// <param name="isGuestRequest">If the user is Guest, this option is ignored. If <see langword="true" />, indicates the cancellation was requested by the guest; otherwise, by a manager/admin.</param>
    /// <returns>HTTP 200 OK on successful cancellation.</returns>
    /// <response code="200">Booking cancelled successfully.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Booking not found.</response>
    /// <response code="409">Booking cannot be cancelled due to conflict with current state.</response>
    /// <response code="422">Unprocessable request entity.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Completes a booking after the guest has finished their visit.
    /// </summary>
    /// <param name="id">The booking ID to complete.</param>
    /// <returns>HTTP 200 OK on successful completion.</returns>
    /// <response code="200">Booking completed successfully.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have the required Manager or Admin role.</response>
    /// <response code="404">Booking not found.</response>
    /// <response code="409">Booking cannot be completed due to conflict with current state.</response>
    /// <response code="422">Unprocessable request entity.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Admin)}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteBooking([FromRoute] Guid id)
    {
        var result = await Mediator.Send(
            new CompleteBookingCommand(BookingId: id));
        if (result.IsFailed) return HandleErrors(result);
        return Ok();
    }

    /// <summary>
    /// Retrieves a specific booking by its <paramref name="id" />.
    /// </summary>
    /// <param name="id">The booking ID to retrieve.</param>
    /// <returns>The booking details.</returns>
    /// <response code="200">Booking retrieved successfully.</response>
    /// <response code="400">Invalid request or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Booking not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Retrieves a paginated list of bookings with optional filters.
    /// </summary>
    /// <param name="query">Filter and pagination parameters.</param>
    /// <returns>A paginated list of bookings matching the filter criteria.</returns>
    /// <response code="200">Bookings retrieved successfully.</response>
    /// <response code="400">Invalid filter parameters or validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBookings([FromQuery] GetBookingsRequest query)
    {
        var result = await Mediator.Send(query.ToQuery());
        if (result.IsFailed) return HandleErrors(result);
        return Ok(result.Value);
    }
}
