using BookingSystem.Domain.Restaurants.ValueObjects;
using JetBrains.Annotations;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantWorkingSchedule;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record GetRestaurantWorkingScheduleResponse(IEnumerable<DayOfWeekSchedule> Schedules);