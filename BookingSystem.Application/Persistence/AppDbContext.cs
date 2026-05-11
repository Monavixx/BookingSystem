using BookingSystem.Application.Persistence.Configurations.Converters;
using BookingSystem.Domain.Common;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.FavoriteRestaurant;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(e => IsEntity(e.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType,
                builder => { builder.Property(nameof(Entity<>.RowVersion)).IsRowVersion(); });
        }
    }
    
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<FavoriteRestaurant> FavoriteRestaurants => Set<FavoriteRestaurant>();
    
    
    private static bool IsEntity(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Entity<>))
                return true;

            type = type.BaseType!;
        }

        return false;
    }
}