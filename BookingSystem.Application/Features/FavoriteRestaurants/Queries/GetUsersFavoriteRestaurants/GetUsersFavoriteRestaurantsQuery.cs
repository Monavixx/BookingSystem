using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.FavoriteRestaurants.Queries.GetUsersFavoriteRestaurants;

public record GetUsersFavoriteRestaurantsQuery(Guid UserId, int Page, int PageSize)
    : IRequest<Result<IEnumerable<PublicRestaurantInfo>>>, IRequireActiveUser;