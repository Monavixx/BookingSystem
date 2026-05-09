using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable(TableNames.Tables);
        builder.HasKey(r => r.Id)
            .HasName(Constraints.PrimaryKey(TableNames.Tables));
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, s => new TableId(s));
        
        builder.Property(r => r.RestaurantId)
            .HasConversion(id => id.Value, s => new RestaurantId(s));
        
        builder.HasOne<Restaurant>()
            .WithMany(r => r.Tables)
            .HasForeignKey(t => t.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName(Constraints.ForeignKey.TablesRestaurant.ConstraintName);
    }
}