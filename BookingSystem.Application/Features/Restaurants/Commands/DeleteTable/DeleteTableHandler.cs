using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Features.Restaurants.Commands.DeleteTable;

public class DeleteTableHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteTableCommand, Result>
{
    public async Task<Result> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        var tableInfo = await dbContext.Tables
            .Where(t => t.RestaurantId == new RestaurantId(request.RestaurantId)
                        && t.TableNumber == request.TableNumber)
            .Select(t => new { Table = t, RestaurantOwnerId = t.Restaurant.OwnerId })
            .FirstOrDefaultAsync(cancellationToken);
        if (tableInfo is null) return TableErrors.NotFound;
        if (tableInfo.RestaurantOwnerId != currentUserService.UserId &&
            (await currentUserService.GetUserAsync())?.Role is not UserRole.Admin)
            return TableErrors.AccessDenied;

        dbContext.Tables.Remove(tableInfo.Table);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Ok(); //todo: tests
    }
}