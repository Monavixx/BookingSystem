using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantWorkingSchedule;

public class GetRestaurantWorkingScheduleHandler(
    AppDbContext dbContext,
    ILogger<GetRestaurantWorkingScheduleHandler> logger) : IRequestHandler<GetRestaurantWorkingScheduleQuery,
    Result<GetRestaurantWorkingScheduleResponse>>
{
    private const string SqlQuery =
        """
        SELECT day_of_week, opening_time, closing_time, is_closed
        FROM restaurant_daily_schedules
        WHERE restaurant_id = @RestaurantId
        ORDER BY day_of_week
        """;

    public async Task<Result<GetRestaurantWorkingScheduleResponse>> Handle(GetRestaurantWorkingScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var schedules = 
            (await connection.QueryAsync<DayOfWeekSchedule>(SqlQuery, new { request.RestaurantId })).AsList();
        if(schedules.Count == 0) return Result.Fail<GetRestaurantWorkingScheduleResponse>(RestaurantErrors.NotFound);
        logger.LogInformation(
            "Retrieved {DayOfWeekScheduleCount} day of week schedules for restaurant with id {RestaurantId} from database",
            schedules.Count,
            request.RestaurantId);
        return new GetRestaurantWorkingScheduleResponse(schedules);
    }
}