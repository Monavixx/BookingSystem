using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Application.Features.Users.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(
    Guid? RestaurantUserBeenTo,
    Guid? RestaurantUserIsAt,
    int? OlderThan,
    int? YoungerThan,
    int? BookingCountGreaterThan,
    int? BookingCountLessThan,
    bool? IsBlocked
) : IRequest<Result<IEnumerable<UserResponse>>>, IRequireActiveUser
{
    public GetUsersQuery() 
        : this(null, null,
            null, null, null,
            null, null)
    { }
}