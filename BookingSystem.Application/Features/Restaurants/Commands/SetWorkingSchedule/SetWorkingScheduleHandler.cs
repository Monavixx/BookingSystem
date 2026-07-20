using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.SetWorkingSchedule;

public class SetWorkingScheduleHandler (AppDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<SetWorkingScheduleCommand, Result>
{
    public async Task<Result> Handle(SetWorkingScheduleCommand request, CancellationToken cancellationToken)
    {
        var restaurant =
            await dbContext.Restaurants.FindAsync([new RestaurantId(request.RestaurantId)], cancellationToken);
        if (restaurant is null) return Result.Fail(RestaurantErrors.NotFound);
        if (restaurant.OwnerId.Value != currentUserService.UserIdGuid)
            return Result.Fail(RestaurantErrors.AccessDenied);

        List<DayOfWeekSchedule> schedules = [];
        var currentDayOfWeek = DayOfWeek.Sunday;
        foreach (var dto in request.Schedules)
        {
            var dayOfWeek = dto.DayOfWeek ?? currentDayOfWeek;
            var resDoWc = DayOfWeekSchedule.Create(dayOfWeek,
                    dto.OpeningTime, dto.ClosingTime, dto.IsClosed ?? false);
            currentDayOfWeek = (DayOfWeek)(((int)dayOfWeek + 1) % 7);
            if (resDoWc.IsFailed) return resDoWc.ToResult();
            schedules.Add(resDoWc.Value);
        }
        
        var workingSchedule = WorkingSchedule.Create(schedules);
        if (workingSchedule.IsFailed) return workingSchedule.ToResult();
        
        restaurant.SetWorkingSchedule(workingSchedule.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}