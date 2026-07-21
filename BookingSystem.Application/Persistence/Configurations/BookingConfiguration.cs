using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Restaurants.Errors;
using BookingSystem.Domain.Restaurants.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, s => new BookingId(s))
            .ValueGeneratedNever();
        builder.HasOne<User>(b=>b.Guest)
            .WithMany()
            .HasForeignKey(b => b.GuestId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(b=>b.GuestId)
            .HasConversion(id => id.Value, s => new UserId(s))
            .ValueGeneratedNever();
        builder.HasOne(b=>b.Table)
            .WithMany()
            .HasForeignKey(b => new { b.RestaurantId, b.TableNumber })
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(b => b.RestaurantId)
            .HasConversion(id => id.Value, s => new RestaurantId(s))
            .ValueGeneratedNever();

        builder.ComplexProperty(b => b.TimeSlot, c =>
        {
            c.Property(t=>t.Start).HasColumnName("start_time");
            c.Property(t=>t.End).HasColumnName("end_time");
        });
        builder.Ignore(b => b.TableId);
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<Booking>(b => b.GuestId, UserErrors.NotFound);
        registry.RegisterForeignKey<Booking>(b => new {b.RestaurantId, b.TableNumber}, TableErrors.NotFound);
    }
}