using BookingSystem.Application.Features.Restaurants.DTOs;
using BookingSystem.Application.Persistence;
using Dapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Application.Features.Restaurants.Queries.GetRestaurantList;

public class GetRestaurantListHandler(AppDbContext dbContext, ILogger<GetRestaurantListHandler> logger)
    : IRequestHandler<GetRestaurantListQuery, Result<IEnumerable<PublicRestaurantInfo>>>
{
    private static string SqlQueryRestaurants(int limit, string? city)
    {
        var limitClause = limit <= 0 ? "" : "LIMIT @Limit OFFSET @Skip\n";
        var filterCityClause = city is null ? "" : "WHERE address_city = @City\n";
        return $"""
                SELECT id, image_url, owner_id, email, description, 
                       address_apartment_number,
                       address_city,
                       address_country,
                       address_house_number,
                       address_state,
                       address_street,
                       address_zip_code,
                       contact_phone_number
                FROM restaurants
                {filterCityClause}
                ORDER BY id 
                {limitClause}
                """;
    }

    private const string SqlQueryTables =
        """
        SELECT t.restaurant_id, t.table_number, t.capacity
        FROM tables t
        WHERE restaurant_id = ANY(@RestaurantIds);
        """;

    public async Task<Result<IEnumerable<PublicRestaurantInfo>>> Handle(GetRestaurantListQuery request,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var restaurants = (await connection.QueryAsync<RestaurantRow>(SqlQueryRestaurants(request.Limit, request.City),
            new
            {
                request.Limit, request.Skip, request.City
            })).ToArray();
        logger.LogInformation("Retrieved {Count} restaurants from database with limit {Limit}, skip {Skip} and city filter {City}",
            restaurants.Length, request.Limit, request.Skip, request.City);

        var tables = (await connection.QueryAsync<TableRow>(SqlQueryTables,
            new
            {
                RestaurantIds = restaurants.Select(r => r.Id).ToArray()
            })).ToLookup(tr => tr.RestaurantId,
            tr => new PublicRestaurantInfo.TableDto(tr.TableNumber, tr.Capacity));
        logger.LogInformation("Retrieved {Count} tables from database for previously retrieved restaurants", tables.Count);

        return Result.Ok<IEnumerable<PublicRestaurantInfo>>(restaurants.Select(r => new PublicRestaurantInfo(
            RestaurantId: r.Id,
            OwnerId: r.OwnerId,
            ImageUrl: r.ImageUrl,
            Description: r.Description,
            Contact: new PublicRestaurantInfo.ContactDto(
                PhoneNumber: r.ContactPhoneNumber,
                Email: r.Email),
            Address: new PublicRestaurantInfo.AddressDto(
                Country: r.AddressCountry,
                City: r.AddressCity,
                State: r.AddressState,
                Street: r.AddressStreet,
                HouseNumber: r.AddressHouseNumber,
                ApartmentNumber: r.AddressApartmentNumber,
                ZipCode: r.AddressZipCode),
            Tables: tables[r.Id].OrderBy(t => t.TableNumber).ToList()
        )).ToArray());
    }

    private record RestaurantRow(
        Guid Id,
        string? ImageUrl,
        Guid OwnerId,
        string Email,
        string? Description,
        string? AddressApartmentNumber,
        string? AddressCity,
        string AddressCountry,
        string? AddressHouseNumber,
        string? AddressState,
        string? AddressStreet,
        string? AddressZipCode,
        string ContactPhoneNumber);

    private record TableRow(
        Guid RestaurantId,
        int TableNumber,
        int Capacity);
}