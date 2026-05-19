using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.User.Errors;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Users.Queries;

public class GetUserHandler(AppDbContext dbContext, ILogger<GetUserHandler> logger, ICurrentUserService currentUserService)
    : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private const string SqlQuery = """
                                     SELECT u.id, u.username, u.email, u.phone_number, 
                                            u.registration_date_time, u.birth_date, u.first_name, u.last_name
                                     FROM users u
                                     WHERE u.id = @UserId
                                     """;

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        logger.LogInformation("Trying to get user with id '{UserId}'", userId);
        var connection = dbContext.Database.GetDbConnection();
        var dto = await connection.QueryFirstOrDefaultAsync<UserDto>(SqlQuery, new { UserId = userId });
        if (dto is null)
        {
            logger.LogWarning("User with id '{UserId}' not found", userId);
            return Result.Fail<UserDto>(UserErrors.NotFound);
        }
        logger.LogInformation("User with id '{UserId}' found", userId);
        return Result.Ok(dto);
    }
}