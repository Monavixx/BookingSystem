using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable(TableNames.Sessions);
        builder.HasKey(s => s.Id)
            .HasName(Constraints.PrimaryKey(TableNames.Sessions));
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, s => new SessionId(s))
            .ValueGeneratedNever();
        builder.ComplexProperty(x => x.RefreshToken, b =>
        {
            b.Property(rt => rt.Token)
                .HasColumnName("RefreshToken")
                .HasMaxLength(RefreshToken.TokenLength)
                .IsRequired();
            b.Property(rt => rt.ExpiresAt)
                .HasColumnName("RefreshTokenExpiresAt")
                .IsRequired();
        });
        
        builder.HasOne<User>()
            .WithMany(u => u.Sessions)
            .HasForeignKey(x=>x.UserId)
            .HasConstraintName(Constraints.ForeignKey.SessionsUser.ConstraintName)
            .OnDelete(DeleteBehavior.Cascade);
    }
}