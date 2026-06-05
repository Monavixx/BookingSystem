using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using Dapper;
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
            return Result.Fail(RestaurantErrors.AccessError);

        List<DayOfWeekSchedule> schedules = [];
        var currentDayOfWeek = DayOfWeek.Sunday;
        foreach (var dto in request.Schedules)
        {
            var resDoWC = DayOfWeekSchedule.Create(dto.DayOfWeek ?? currentDayOfWeek++,
                    dto.OpeningTime, dto.ClosingTime, dto.IsClosed ?? false);
            if (resDoWC.IsFailed) return resDoWC.ToResult();
            schedules.Add(resDoWC.Value);
        }
        
        var workingSchedule = WorkingSchedule.Create(schedules);
        if (workingSchedule.IsFailed) return workingSchedule.ToResult();
        
        restaurant.SetWorkingSchedule(workingSchedule.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Result.Ok();
    }
}