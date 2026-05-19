using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.Errors;
using BookingSystem.Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, s => new SessionId(s))
            .ValueGeneratedNever();
        builder.ComplexProperty(x => x.RefreshToken, b =>
        {
            b.Property(rt => rt.Token)
                .HasMaxLength(RefreshToken.TokenLength)
                .IsRequired();
            b.Property(rt => rt.ExpiresAt)
                .IsRequired();
        });

        builder.HasOne<User>(session => session.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<Session>(session => session.UserId, UserErrors.NotFound);
    }
}