using BookingSystem.Application.Persistence.Abstractions;
using BookingSystem.Application.Persistence.Configurations.Converters;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Users;
using BookingSystem.Domain.Users.Errors;
using BookingSystem.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>, IConstraintErrorConfiguration
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();
        
        builder.Navigation(x => x.FavoriteRestaurants)
            .UsePropertyAccessMode(PropertyAccessMode.Field); // IMPORTANT
        builder.Navigation(x=>x.Sessions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder.Property(x => x.FirstName)
            .HasMaxLength(User.FirstNameMaxLength)
            .IsRequired();
        builder.Property(x => x.LastName)
            .HasMaxLength(User.LastNameMaxLength)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasConversion<EmailAddressConverter>()
            .HasMaxLength(EmailAddress.MaxLength)
            .IsRequired();
        builder.HasIndex(x => x.Email)
            .IsUnique();
        
        builder.Property(x => x.PhoneNumber)
            .HasConversion<PhoneNumberConverter>()
            .HasMaxLength(PhoneNumber.MaxLength)
            .IsRequired();
        builder.HasIndex(x => x.PhoneNumber)
            .IsUnique();
        
        builder.Property(x => x.Username)
            .HasConversion(username => username.Value, value => Username.Create(value).Value)
            .HasMaxLength(Username.MaxLength)
            .IsRequired();
        builder.HasIndex(x => x.Username)
            .IsUnique();
        
        builder.OwnsOne(u => u.BirthDate, birthDateBuilder =>
        {
            birthDateBuilder.Property(b => b.Value)
                .HasColumnName("birth_date")
                .HasColumnType("date")
                .IsRequired();
        });
        builder.Navigation(u => u.BirthDate).IsRequired();
        
        builder.Property(x=>x.PasswordHash)
            .HasMaxLength(User.PasswordHashLength)
            .IsRequired();
    }

    public void Configure(ConstraintErrorRegistryBase registry)
    {
        registry
            .RegisterUnique<User>(u => u.Username, UserErrors.Username.AlreadyInUse)
            .RegisterUnique<User>(u => u.Email, UserErrors.Email.AlreadyInUse)
            .RegisterUnique<User>(u => u.PhoneNumber, UserErrors.PhoneNumber.AlreadyInUse);
    }
}