using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.Errors;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetPublicRestaurantInfo;

public class GetPublicRestaurantInfoHandler (AppDbContext dbContext, ILogger<GetPublicRestaurantInfoHandler> logger) : IRequestHandler<GetPublicRestaurantInfoQuery, Result<GetPublicRestaurantInfoResponse>>
{
    private const string SqlQuery =
        """
        SELECT image_url, owner_id, email, description, 
               address_apartment_number,
               address_city,
               address_country,
               address_house_number,
               address_state,
               address_street,
               address_zip_code,
               contact_phone_number
        FROM restaurants
        WHERE id = @RestaurantId
        LIMIT 1;

        SELECT table_number, capacity
        FROM tables
        WHERE restaurant_id = @RestaurantId;
        """;
    public async Task<Result<GetPublicRestaurantInfoResponse>> Handle(GetPublicRestaurantInfoQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var reader = await connection.QueryMultipleAsync(SqlQuery, new { RestaurantId = request.RestaurantId });
        var restaurantResult = await reader.ReadFirstOrDefaultAsync();
        if (restaurantResult is null) return Result.Fail<GetPublicRestaurantInfoResponse>(RestaurantErrors.NotFound);
        
        return new GetPublicRestaurantInfoResponse(
            RestaurantId: request.RestaurantId,
            ImageUrl: restaurantResult.image_url,
            Description:restaurantResult.description,
            Contact: new GetPublicRestaurantInfoResponse.ContactDto(
                PhoneNumber: restaurantResult.contact_phone_number, 
                Email: restaurantResult.email),
            Address: new GetPublicRestaurantInfoResponse.AddressDto(
                Country: restaurantResult.address_country,
                City: restaurantResult.address_city,
                State: restaurantResult.address_state,
                Street: restaurantResult.address_street,
                HouseNumber: restaurantResult.address_house_number,
                ApartmentNumber: restaurantResult.address_apartment_number,
                ZipCode: restaurantResult.address_zip_code),
            Tables: (await reader.ReadAsync<GetPublicRestaurantInfoResponse.TableDto>()).AsList()
        );
    }
}