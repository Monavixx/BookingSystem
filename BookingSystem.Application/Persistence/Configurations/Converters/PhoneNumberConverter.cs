using BookingSystem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookingSystem.Application.Persistence.Configurations.Converters;

public class PhoneNumberConverter() : ValueConverter<PhoneNumber, string>
(phone => phone.Value, value => PhoneNumber.Create(value).Value);