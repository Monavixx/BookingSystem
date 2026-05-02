using BookingSystem.Application.Persistence.Configurations.Converters;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.FavoriteRestaurant;
using BookingSystem.Domain.User;
using BookingSystem.Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();
        
        builder.Navigation(x => x.FavoriteRestaurants)
            .UsePropertyAccessMode(PropertyAccessMode.Field); // IMPORTANT
        
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
        builder.Property(x => x.Username)
            .HasConversion(username => username.Value, value => Username.Create(value).Value)
            .HasMaxLength(Username.MaxLength)
            .IsRequired();
        builder.Property(x => x.BirthDate)
            .HasConversion(birthDate => birthDate.Value, value => Birthdate.Create(value).Value)
            .IsRequired();
        builder.Property(x => x.RegistrationDateTime)
            .HasConversion(registrationDateTime => registrationDateTime.Value, value => new RegistrationDateTime(value))
            .IsRequired();
        builder.Property(x=>x.PasswordHash)
            .HasMaxLength(128+16)
            .IsRequired();
        
        builder.HasMany(x=>x.Sessions)
            .WithOne()
            .HasForeignKey(x=>x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x=>x.Sessions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}