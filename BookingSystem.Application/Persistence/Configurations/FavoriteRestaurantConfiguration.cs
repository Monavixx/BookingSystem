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
        builder.ToTable(TableNames.FavoriteRestaurants);
        
        builder.HasKey(fr => new { fr.UserId, fr.RestaurantId })
            .HasName(Constraints.PrimaryKey(TableNames.FavoriteRestaurants));
        
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
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName(Constraints.ForeignKey.FavoriteRestaurantsUser.ConstraintName);
        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(fr => fr.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName(Constraints.ForeignKey.FavoriteRestaurantsRestaurant.ConstraintName);
    }
}