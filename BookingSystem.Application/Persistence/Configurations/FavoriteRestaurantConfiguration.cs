using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.FavoriteRestaurant;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.Errors;
using BookingSystem.Domain.User.ValueObjects;
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
        builder.HasOne<Restaurant>()
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