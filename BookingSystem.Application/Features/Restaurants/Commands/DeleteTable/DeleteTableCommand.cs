using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.DeleteTable;

public sealed record DeleteTableCommand(Guid RestaurantId, int TableNumber) : IRequest<Result>;