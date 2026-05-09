using BookingSystem.Application.Persistence.Configurations.Converters;
using BookingSystem.Domain.Common.ValueObjects;
using BookingSystem.Domain.Restaurant;
using BookingSystem.Domain.Restaurant.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Application.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable(TableNames.Restaurants);
        builder.HasKey(x => x.Id)
            .HasName(Constraints.PrimaryKey(TableNames.Restaurants));
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, s => new RestaurantId(s));
        builder.ComplexProperty<Address>(r => r.Address, b =>
        {
            b.Property(a => a.Country).HasMaxLength(Address.CountryMaxLength).IsRequired();
            b.Property(a => a.State).HasMaxLength(Address.StateMaxLength).IsRequired(false);
            b.Property(a => a.City).HasMaxLength(Address.CityMaxLength).IsRequired(false);
            b.Property(a => a.Street).HasMaxLength(Address.StreetMaxLength).IsRequired(false);
            b.Property(a => a.HouseNumber).HasMaxLength(Address.HouseNumberMaxLength).IsRequired(false);
            b.Property(a => a.ApartmentNumber).HasMaxLength(Address.ApartmentNumberMaxLength).IsRequired(false);
            b.Property(a => a.ZipCode).HasMaxLength(Address.ZipCodeMaxLength).IsRequired(false);
        });
        builder.Property(r => r.ContactPhoneNumber)
            .HasConversion<PhoneNumberConverter>()
            .HasColumnName("ContactPhoneNumber")
            .HasMaxLength(PhoneNumber.MaxLength);
        builder.Property(r => r.Email)
            .HasConversion<EmailAddressConverter>()
            .HasColumnName("Email")
            .HasMaxLength(EmailAddress.MaxLength);
        builder.Property(r => r.Description)
            .HasMaxLength(Restaurant.DescriptionMaxLength);
        builder.Property(r => r.ImageUrl)
            .HasConversion<UrlConverter>()
            .HasColumnName("ImageUrl")
            .HasMaxLength(Url.MaxLength);
    }
}