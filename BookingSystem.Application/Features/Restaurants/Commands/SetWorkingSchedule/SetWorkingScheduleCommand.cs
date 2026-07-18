using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Features.Restaurants.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.SetWorkingSchedule;

public sealed record SetWorkingScheduleCommand(Guid RestaurantId, IEnumerable<DayOfWeekScheduleRequest> Schedules)
    : IRequest<Result>, IRequireActiveUser;