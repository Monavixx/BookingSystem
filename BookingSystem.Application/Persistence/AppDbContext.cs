using BookingSystem.Application.Persistence.Interceptors;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Common;
using BookingSystem.Domain.FavoriteRestaurants;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Application.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, IPublisher  publisher)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (IsEntity(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType,
                    builder => builder.Property(nameof(IEntity.RowVersion)).IsRowVersion());
            }
            if (entityType.ClrType.IsAssignableTo(typeof(IAggregateRoot)))
            {
                modelBuilder.Entity(entityType.ClrType,
                    builder => builder.Ignore(nameof(IAggregateRoot.DomainEvents)));
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new DomainEventInterceptor(publisher));
        base.OnConfiguring(optionsBuilder);
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<FavoriteRestaurant> FavoriteRestaurants => Set<FavoriteRestaurant>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<CancellationRecord> CancellationRecords => Set<CancellationRecord>();
    
    
    private static bool IsEntity(Type type)
        => type.IsAssignableTo(typeof(IEntity));
}