using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DependencyInjection;
public static class AttributeServiceCollectionExtension
{
    /// <summary>
    /// <inheritdoc cref="ServiceAttribute"/>>
    /// references:
    /// <list type="bullet">
    /// <item> <seealso cref="ServiceCollectionHelper.AddAlias"/> </item>
    /// <item><seealso cref="ServiceAttribute"/></item>
    /// <item><seealso cref="ServiceImplementationOverrideAttribute"/></item>
    /// </list>
    ///
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ServiceAttribute"/>
    /// <inheritdoc cref="ServiceImplementationOverrideAttribute"/>
    /// </remarks>
    public static IServiceCollection AddServicesWithAttributes(this IServiceCollection services, ITypeCache typeCache, ExceptionBuilder exceptions)
    {
        exceptions = exceptions.CreateChild(nameof(AddServicesWithAttributes));

        // contains all types which either have the service attribute directly or have an interface with the service attribute
        var serviceDetails = typeCache
            .KnownTypes
            .Where(t => t.IsClass)
            .Select(t => new
            {
                Implementation = t,
                Services = t
                    .GetInterfaces()
                    .Where(x => x.HasCustomAttribute<ServiceAttribute>())
                    .ToArray(),
                isOverride = t.HasCustomAttribute<ServiceImplementationOverrideAttribute>(),
                overriddenImplementation = t.GetCustomAttribute<ServiceImplementationOverrideAttribute>()?.MatchServiceInterfacesOfType
            })
            .Where(x => x.Services.Any() || x.Implementation.HasCustomAttribute<ServiceAttribute>())
            .ToReadOnlyCollection();

        var overrides = serviceDetails
            .Where(x => x.isOverride)
            .ToReadOnlyCollection();

        // override does neither have a service with the service attribute nor has it itself
        exceptions
            .CreateChild("override implementations are not associated to any service")
            .Add(typeCache.KnownTypes
                .Where(t => t.HasCustomAttribute<ServiceImplementationOverrideAttribute>())
                .Except(overrides.Select(o => o.Implementation))
                .Select(invalidOverride => new InvalidSetupException(invalidOverride.FullName()))
            );

        // check for missing service interfaces in override
        foreach (var @override in overrides.Where(o => o.overriddenImplementation is not null))
        {
            var subEx = exceptions.CreateChild($"override {@override.Implementation.FullName()} is invalid");

            var overridden = serviceDetails
                .SingleOrDefault(s => s.Implementation == @override.overriddenImplementation);
            if (overridden is null)
            {
                subEx.Add(new InvalidSetupException(
                    $"implementation is set to match ${@override.overriddenImplementation?.FullName()}, but this class does not have any services associated"));
                continue;
            }
            exceptions
                .CreateChild("missing services")
                .Add(overridden.Services.Except(@override.Services)
                    .Select(overriddenService => new InvalidSetupException(overriddenService.FullName()))
                );
        }

        // add the services to the collection
        foreach (var serviceDetail in serviceDetails)
        {
            try
            {
                var implementationType = serviceDetail.Implementation;
                var serviceTypes = serviceDetail.Services;

                var implementationLifetime = implementationType.GetCustomAttribute<ServiceAttribute>()?.ServiceLifetime;

                switch (serviceTypes.Length)
                {
                    case 0:
                        services.Add(new(
                            implementationType,
                            implementationType,
                            implementationLifetime
                                ?? throw new InvalidSetupException($"{implementationType.FullName(true)} service implementation lifetime not found")
                            ));
                        break;
                    case 1 when implementationLifetime.HasValue == false
                                && serviceDetail.isOverride == false // do not add the service if it is overridden
                                && overrides.None(o => o.Implementation == implementationType):

                        services.Add(new(
                            serviceTypes.Single(),
                            implementationType,
                            serviceTypes
                                .Single()
                                .GetCustomAttribute<ServiceAttribute>()?.ServiceLifetime
                                    ?? throw new InvalidSetupException($"{implementationType.FullName(true)} => {serviceTypes.Single().FullName(true)} service interface lifetime not found")
                          ));
                        break;
                    case 1: // when implementationLifetime.HasValue
                    case > 1:
                        implementationLifetime ??= serviceTypes.Min(st =>
                            st.GetCustomAttribute<ServiceAttribute>()?.ServiceLifetime);// gets the highest required scope

                        if (implementationLifetime.HasValue == false)
                        {
                            exceptions.Add(new InvalidSetupException(
                                $"{implementationType.FullName(true)} => {serviceTypes.Single().FullName(true)} service implementation lifetime not found"));
                            break;
                        }
                    
                        bool anyServiceProvided = false;
                        foreach (var serviceType in serviceTypes)
                        {
                            // override handling
                            if (!serviceDetail.isOverride
                                // overrides contains any implementation of this service
                                && overrides.Any(o => o.Services.Contains(serviceType))
                                )
                            {
                                continue;
                            }

                            try
                            {
                                var serviceLifetime = serviceType.GetCustomAttribute<ServiceAttribute>()?.ServiceLifetime;
                                if (serviceLifetime.HasValue == false)
                                {
                                    exceptions.Add(new InvalidSetupException(
                                        $"{implementationType.FullName(true)} => {serviceType.FullName(true)} service interface lifetime not found"));
                                    continue;
                                }
                                if (serviceLifetime < implementationLifetime)
                                {
                                    exceptions.Add(new InvalidSetupException(
                                        $"{implementationType.FullName(true)} => {serviceType.FullName(true)} service interface lifetime {serviceLifetime} is less than implementation lifetime {implementationLifetime}"));
                                    continue;
                                }
                                services.AddAlias(
                                    serviceType,
                                    implementationType,
                                    serviceLifetime.Value
                                );
                                anyServiceProvided = true;

                            }
                            catch (Exception e)
                            {
                                exceptions.Add(e);
                            }
                        }

                        // provide the implementation only, if there is any service referencing it, or it has the service attribute
                        if (anyServiceProvided ||
                            implementationType.HasCustomAttribute<ServiceImplementationOverrideAttribute>())
                        {
                            // the implementation provides itself, so that all interfaces can share the same instance
                            services.Add(new(
                                implementationType,
                                implementationType,
                                implementationLifetime.Value));
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        return services;
    }
}
