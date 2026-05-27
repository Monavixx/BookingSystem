using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using BookingSystem.Domain.User.ValueObjects;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantHandler (AppDbContext dbContext, ILogger<UpdateRestaurantHandler> logger,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateRestaurantCommand, Result>
{
    private record RestaurantRow(Guid OwnerId, uint RowVersion);
    public async Task<Result> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var oldRestaurant = await dbContext.Database.GetDbConnection().QueryFirstOrDefaultAsync<RestaurantRow>(
            """
            SELECT owner_id, xmin as RowVersion FROM Restaurants WHERE id = @Id
            """, new { Id = request.RestaurantId });
        if (oldRestaurant is null) return Result.Fail(RestaurantErrors.NotFound);
        
        if (currentUserService.UserIdGuid is not { } userId || userId != oldRestaurant.OwnerId)
            return Result.Fail(RestaurantErrors.AccessError);

        var newRestaurant = Restaurant.Create(
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
            new UserId(oldRestaurant.OwnerId),
            new RestaurantId(request.RestaurantId)
        );
        if (newRestaurant.IsFailed) return newRestaurant.ToResult();
        newRestaurant.Value.RowVersion = oldRestaurant.RowVersion;
        
        dbContext.Update(newRestaurant.Value);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Restaurant {RestaurantId} updated successfully", request.RestaurantId);
        return Result.Ok();
    }
}