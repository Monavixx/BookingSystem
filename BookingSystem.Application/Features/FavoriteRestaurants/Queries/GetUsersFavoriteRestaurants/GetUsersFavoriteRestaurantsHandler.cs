using BookingSystem.Application.Common.DTOs;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users.ValueObjects;
using Dapper;
using FluentResults;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.FavoriteRestaurants.Queries.GetUsersFavoriteRestaurants;

public class GetUsersFavoriteRestaurantsHandler(AppDbContext dbContext)
    : IRequestHandler<GetUsersFavoriteRestaurantsQuery,
        Result<IEnumerable<PublicRestaurantInfo>>>
{
    public async Task<Result<IEnumerable<PublicRestaurantInfo>>> Handle(GetUsersFavoriteRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        // var connection = dbContext.Database.GetDbConnection();
        // var res = await connection.QueryAsync<PublicRestaurantInfo>(
        //     """
        //     SELECT r. FROM favorite_restaurants fr
        //     JOIN restaurants r ON fr.restaurant_id = r.id
        //     WHERE fr.user_id = @UserId
        //     """);
        var res = (await dbContext.FavoriteRestaurants
            .Where(fr => fr.UserId == new UserId(request.UserId))
            .Select(fr => new
            {
                Restaurant = fr.Restaurant,
                Tables = fr.Restaurant.Tables.ToList()
            })
            .ToArrayAsync(cancellationToken))
            .Select(r => new PublicRestaurantInfo(
                RestaurantId: r.Restaurant.Id.Value,
                OwnerId: r.Restaurant.OwnerId.Value,
                Address: PublicRestaurantInfo.AddressDto.FromAddress(r.Restaurant.Address),
                Contact: new PublicRestaurantInfo.ContactDto(
                    PhoneNumber: r.Restaurant.ContactPhoneNumber.Value,
                    Email: r.Restaurant.Email.Value),
                Description: r.Restaurant.Description,
                ImageUrl: r.Restaurant.ImageUrl?.Value,
                Tables: r.Tables.Select(t =>
                    new PublicRestaurantInfo.TableDto(t.TableNumber, t.Capacity))))
            .ToArray();
        return res;
    }
}