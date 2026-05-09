using System.Collections.Frozen;
using BookingSystem.Application.Common.Abstractions;
using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Common.Errors;
using FluentResults;
using Npgsql;

namespace BookingSystem.Infrastructure.Services;

internal class ConstraintViolationMapper : IConstraintViolationMapper
{
    private static readonly FrozenDictionary<ConstraintData, IError> UniqueConstraintErrors =
        new Dictionary<ConstraintData, IError>
        {
            [Constraints.Unique.UserUsername] =
                new ConflictError("User.Username.AlreadyExists", "Username is already in use"),
            [Constraints.Unique.UserEmail] = new ConflictError("User.Email.AlreadyExists", "Email is already in use"),
            [Constraints.Unique.UserPhoneNumber] =
                new ConflictError("User.PhoneNumber.AlreadyExists", "Phone number is already in use")
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<ConstraintData, IError> ForeignKeyConstraintErrors =
        new Dictionary<ConstraintData, IError>
        {
            [Constraints.ForeignKey.SessionsUser] =
                new ValidationError("Session.UserId.NotFound", "The specified user does not exist"),
            [Constraints.ForeignKey.FavoriteRestaurantsRestaurant] =
                new ValidationError("FavoriteRestaurant.RestaurantId.NotFound",
                    "The specified restaurant does not exist"),
            [Constraints.ForeignKey.FavoriteRestaurantsUser] = new ValidationError("FavoriteRestaurant.UserId.NotFound",
                "The specified user does not exist"),
            [Constraints.ForeignKey.TablesRestaurant] = new ValidationError("Table.RestaurantId.NotFound",
                "The specified restaurant does not exist"),
        }.ToFrozenDictionary();

    public IError MapUniqueConstraintViolation(string constraintName, string tableName)
    {
        if (Constraints.IsPrimaryKeyConstraint(constraintName))
            return new ConflictError($"{tableName}.Id.AlreadyExists",
                $"An entity with the same id already exists in {tableName}");
        var key = new ConstraintData(constraintName, tableName);
        return UniqueConstraintErrors.TryGetValue(key, out var error)
            ? error
            : UnexpectedDatabaseError();
    }

    public IError MapForeignKeyConstraintViolation(string constraintName, string tableName)
    {
        var key = new ConstraintData(constraintName, tableName);
        return ForeignKeyConstraintErrors.TryGetValue(key, out var error)
            ? error
            : UnexpectedDatabaseError();
    }

    private static readonly IError UnexpectedDatabaseErrorInstance =
        new InternalServerError("UnexpectedDatabaseError", "An unexpected database error occurred");

    public IError UnexpectedDatabaseError() => UnexpectedDatabaseErrorInstance;

    public IError MapConstraintViolation(string sqlState, string constraintName, string tableName)
        => sqlState switch
        {
            PostgresErrorCodes.UniqueViolation => MapUniqueConstraintViolation(constraintName, tableName),
            PostgresErrorCodes.ForeignKeyViolation => MapForeignKeyConstraintViolation(constraintName, tableName),
            _ => UnexpectedDatabaseError()
        };
}