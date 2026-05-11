namespace BookingSystem.Application.Persistence.Abstractions;

public interface IConstraintErrorConfiguration
{
    void Configure(ConstraintErrorRegistryBase registry);
}