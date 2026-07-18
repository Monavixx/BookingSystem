using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.DeleteRestaurant;

public record DeleteRestaurantCommand(Guid RestaurantId) : IRequest<Result>, IRequireActiveUser;