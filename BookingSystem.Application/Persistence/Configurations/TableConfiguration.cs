using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, s => new TableId(s));
        builder.Property(r=>r.RestaurantId)
            .HasConversion(id => id.Value, s => new RestaurantId(s));
        builder.HasOne<Restaurant>()
            .WithMany()
            .HasForeignKey(t => t.RestaurantId);
    }
}