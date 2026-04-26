using BookingSystem.Domain.FavoriteRestaurant;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.ValueObjects;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class FavoriteRestaurantConfiguration : IEntityTypeConfiguration<FavoriteRestaurant>
{
    public void Configure(EntityTypeBuilder<FavoriteRestaurant> builder)
    {
        builder.ToTable("FavoriteRestaurants");
        builder.HasKey(fr => new { fr.UserId, fr.RestaurantId });
        builder.Property(fr => fr.UserId)
            .HasConversion(id => id.Value, s => new UserId(s));
        builder.Property(fr => fr.RestaurantId)
            .HasConversion(id => id.Value, s => new RestaurantId(s));
        builder.HasOne<User>()
            .WithMany(u => u.FavoriteRestaurants)
            .HasForeignKey(fr => fr.UserId);
        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(fr => fr.RestaurantId);
    }
}