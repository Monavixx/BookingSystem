using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.FavoriteRestaurants.Commands.RemoveRestaurantFromFavorites;

public record RemoveRestaurantFromFavoritesCommand(Guid RestaurantId) : IRequest<Result>, IRequireActiveUser;