using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.FavoriteRestaurants.Commands.AddRestaurantToFavorites;

public record AddRestaurantToFavoritesCommand(Guid RestaurantId) : IRequest<Result>, IRequireActiveUser;