using BookingSystem.Application.Common.PipelineBehaviors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Commands.DeleteTable;

public sealed record DeleteTablesCommand(IEnumerable<TableId> Commands) : IRequest<Result>, IRequireActiveUser;