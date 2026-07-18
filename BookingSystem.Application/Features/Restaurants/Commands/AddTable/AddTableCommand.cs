using BookingSystem.Application.Common.PipelineBehaviors;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.AddTable;

public record AddTableCommand(Guid RestaurantId, int TableNumber, int Capacity) : IRequest<Result>, IRequireActiveUser;