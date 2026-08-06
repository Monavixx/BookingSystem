using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantHandler(AppDbContext dbContext, ILogger<CreateRestaurantHandler> logger,
    IReadOnlyCurrentUserService currentUserService)
    : IRequestHandler<CreateRestaurantCommand, Result<CreateRestaurantResult>>
{
    public async Task<Result<CreateRestaurantResult>> Handle(CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurantResult = Restaurant.Create(
            request.Address.Country,
            request.Address.State,
            request.Address.City,
            request.Address.Street,
            request.Address.HouseNumber,
            request.Address.ApartmentNumber,
            request.Address.ZipCode,
            request.Contact.PhoneNumber,
            request.Contact.Email,
            request.Description,
            request.ImageUrl,
            currentUserService.GetRequiredUserId());
        if (restaurantResult.IsFailed) return restaurantResult.ToResult<CreateRestaurantResult>();

        var restaurant = restaurantResult.Value;

        dbContext.Restaurants.Add(restaurant);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Restaurant with id {RestaurantId} created", restaurant.Id);

        return new CreateRestaurantResult(restaurant.Id.Value);
    }
}
