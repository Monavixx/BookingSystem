namespace BookingSystem.Application.Persistence;

public enum ConstraintViolationType
{
    Unique,
    ForeignKey,
    Check,
    NotNull,
    Other
}