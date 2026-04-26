using BookingSystem.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookingSystem.Application.Persistence.Configurations.Converters;

public class UrlConverter() : ValueConverter<Url, string>(url => url.Value, s => Url.Create(s).Value);