using FluentResults;
using Npgsql;

namespace BookingSystem.Application.Common.Abstractions;

public interface IConstraintViolationMapper
{
    IError MapUniqueConstraintViolation(string constraintName, string tableName);
    IError MapForeignKeyConstraintViolation(string constraintName, string tableName);
    IError UnexpectedDatabaseError();
    IError MapConstraintViolation(string sqlState, string constraintName, string tableName);
}