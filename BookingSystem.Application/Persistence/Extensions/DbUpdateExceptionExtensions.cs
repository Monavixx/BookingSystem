using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingSystem.Application.Persistence.Extensions;

public static class DbUpdateExceptionExtensions
{
    public static string? Constraint(this DbUpdateException ex)
        => ((PostgresException)ex.InnerException!).ConstraintName;
}