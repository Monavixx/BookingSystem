using BookingSystem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookingSystem.Application.Persistence.Configurations.Converters;

public class EmailAddressConverter() : ValueConverter<EmailAddress, string>
(email => email.Value, value => EmailAddress.Create(value).Value);