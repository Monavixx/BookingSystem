using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Domain.Bookings;
using BookingSystem.Domain.Bookings.Errors;
using BookingSystem.Domain.Bookings.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class CancellationRecordConfiguration : IEntityTypeConfiguration<CancellationRecord>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<CancellationRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.WhoCancelledId)
            .HasConversion(x => x == null ? null : (Guid?)x.Value.Value,
                x => x == null ? null : new UserId(x.Value));
        builder.Property(x => x.BookingId)
            .HasConversion(x => x == null ? null : (Guid?)x.Value.Value,
                x => x == null ? null : new BookingId(x.Value));

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.WhoCancelledId);
        builder.HasOne<Booking>()
            .WithOne(b => b.CancellationRecord)
            .HasForeignKey<CancellationRecord>(x => x.BookingId);
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry.RegisterForeignKey<CancellationRecord>(cr => cr.BookingId, BookingErrors.NotFound);
        registry.RegisterForeignKey<CancellationRecord>(cr => cr.WhoCancelledId, UserErrors.NotFound);
        registry.RegisterUnique<CancellationRecord>(cr => cr.BookingId,
            CancellationRecordErrors.RecordOfThisBookingAlreadyExists);
    }
}