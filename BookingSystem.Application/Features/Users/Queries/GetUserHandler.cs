using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Common.Errors;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Users.Queries;

public class GetUserHandler (AppDbContext dbContext) : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private static readonly IError UserNotFoundError = new NotFoundError("User.NotFound", "User not found");

    private const string SqlQuery = $"""
                                     SELECT u.id, u.username, u.email, u.phone_number, 
                                            u.registration_date_time, u.birth_date, u.first_name, u.last_name
                                     FROM users u
                                     WHERE u.id = @{nameof(GetUserQuery.UserId)}
                                     """;

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var dto = await connection.QueryFirstOrDefaultAsync<UserDto>(SqlQuery, request);
        
        if (dto is null) return Result.Fail<UserDto>(UserNotFoundError);
        
        return Result.Ok(dto);
    }
}