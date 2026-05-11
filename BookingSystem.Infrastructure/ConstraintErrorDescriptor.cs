using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Common.Errors;

namespace BookingSystem.Infrastructure;

public sealed record ConstraintErrorDescriptor(
    Type ClrEntityType,
    IReadOnlyList<string> PropertyNames,
    DomainError Error,
    ConstraintViolationType ConstraintViolationType
    );