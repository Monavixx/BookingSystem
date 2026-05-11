namespace BookingSystem.Infrastructure.Exceptions;

public class InvalidConstraintErrorDescriptorException (string detail) : Exception(detail);