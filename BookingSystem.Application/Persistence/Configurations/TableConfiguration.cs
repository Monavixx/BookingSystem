using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.Errors;
using BookingSystem.Domain.Restaurant.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, s => new TableId(s));
        
        builder.Property(r => r.RestaurantId)
            .HasConversion(id => id.Value, s => new RestaurantId(s));

        builder.HasOne<Restaurant>()
            .WithMany(r => r.Tables)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<Table>(table => table.RestaurantId, RestaurantErrors.NotFound);
    }
}