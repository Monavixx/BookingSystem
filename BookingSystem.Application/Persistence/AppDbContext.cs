using BookingSystem.Application.Persistence.Configurations.Converters;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.User;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext (options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<User> Users => Set<User>();
}