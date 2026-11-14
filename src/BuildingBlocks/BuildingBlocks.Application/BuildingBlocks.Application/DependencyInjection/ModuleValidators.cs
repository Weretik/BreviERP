using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application.DependencyInjection;

public static class ModuleValidators
{
    /// <summary>
    /// Registers a module's request validators. A child validator that a parent builds with <c>new</c> and that takes
    /// values (a language, a field name) in its constructor is not registered: the container could never build it.
    /// </summary>
    public static IServiceCollection AddModuleValidators(this IServiceCollection services, Assembly applicationAssembly) =>
        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true,
            filter: result => result.ValidatorType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(constructor => constructor.GetParameters().All(parameter =>
                    !parameter.ParameterType.IsPrimitive && parameter.ParameterType != typeof(string))));
}
