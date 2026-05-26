using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.AddTableToRestaurant;

public record AddTableToRestaurantCommand(Guid RestaurantId, int TableNumber, int Capacity) : IRequest<Result>;