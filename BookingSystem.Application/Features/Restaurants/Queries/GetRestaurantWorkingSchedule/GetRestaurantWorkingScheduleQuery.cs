using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantWorkingSchedule;

public sealed record GetRestaurantWorkingScheduleQuery
(Guid RestaurantId) : IRequest<Result<GetRestaurantWorkingScheduleResponse>>;