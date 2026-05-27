using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Features.Restaurants.DTOs;
using FluentResults;
using MediatR;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantList;

public record GetRestaurantListQuery(int Limit, int Skip, string? City) : IRequest<Result<IEnumerable<PublicRestaurantInfo>>>;