using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Restaurants;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.HasKey(r => new { r.RestaurantId, r.TableNumber });
        
        builder.Property(r => r.RestaurantId)
            .HasConversion(id => id.Value, s => new RestaurantId(s));

        builder.HasOne(t=>t.Restaurant)
            .WithMany(r => r.Tables)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        builder.Ignore(t => t.Id);
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<Table>(table => table.RestaurantId, RestaurantErrors.NotFound);
    }
}