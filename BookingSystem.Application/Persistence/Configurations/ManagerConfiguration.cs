using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.HasKey(m => m.UserId);
        builder.Property(m => m.UserId)
            .HasConversion(id => id.Value, value => new UserId(value));
        builder.HasOne(m => m.User)
            .WithOne()
            .HasForeignKey<Manager>(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.Restaurants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<Manager>(m => m.UserId, UserErrors.NotFound);
    }
}