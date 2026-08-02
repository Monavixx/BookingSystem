using BookingSystem.Application.Common.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetPublicRestaurantInfo;

public record GetPublicRestaurantInfoQuery(Guid RestaurantId) : IRequest<Result<PublicRestaurantInfo>>;
