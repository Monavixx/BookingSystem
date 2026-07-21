using System.Reflection;
using BookingSystem.Application.Persistence.Abstractions;

namespace BookingSystem.Application.Persistence.Extensions;

public static class ConstraintErrorRegistryExtensions
{
    public static ConstraintErrorRegistryBase AddConstraintErrorsFromAssembly(
        this ConstraintErrorRegistryBase registryBase,
        Assembly assembly)
    {
        var types = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                        t.IsAssignableTo(typeof(IConstraintErrorConfiguration)));

        foreach (var type in types)
        {
            var configuration = (IConstraintErrorConfiguration)Activator.CreateInstance(type)!;
            configuration.Configure(registryBase);
        }

        return registryBase;
    }
}