using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.FavoriteRestaurants;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class FavoriteRestaurantConfiguration : IEntityTypeConfiguration<FavoriteRestaurant>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<FavoriteRestaurant> builder)
    {
        builder.HasKey(fr => new { fr.UserId, fr.RestaurantId });

        builder.Property(fr => fr.UserId)
            .HasConversion(id => id.Value, s => new UserId(s))
            .IsRequired()
            .ValueGeneratedNever();
        builder.Property(fr => fr.RestaurantId)
            .IsRequired()
            .HasConversion(id => id.Value, s => new RestaurantId(s))
            .ValueGeneratedNever();

        builder.HasOne<User>()
            .WithMany(u => u.FavoriteRestaurants)
            .HasForeignKey(fr => fr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Restaurant>(fr => fr.Restaurant)
            .WithMany()
            .HasForeignKey(fr => fr.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<FavoriteRestaurant>(fr => fr.RestaurantId, RestaurantErrors.NotFound);
        registry.RegisterForeignKey<FavoriteRestaurant>(fr => fr.UserId, UserErrors.NotFound);
    }
}