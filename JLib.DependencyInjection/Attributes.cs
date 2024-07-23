using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DependencyInjection;

/// <summary>
/// Registers this service to be added as a service with the given <see cref="ServiceLifetime"/>.
/// can be provided as service using <see cref="AttributeServiceCollectionExtension.AddServicesWithAttributes"/>
/// adds types with the <see cref="ServiceAttribute"/> to the service collection.<br/>
/// if it is a class, it will be treated as implementation and will always be provided with the <see cref="ServiceLifetime"/> of <see cref="ServiceAttribute.ServiceLifetime"/><br/>
/// if it is an interface, each class implementing this interface will be added.<br/>
/// If the implementation has no <see cref="ServiceAttribute"/>, the implementation will be provided with the <see cref="ServiceLifetime"/> of the interface.<br/>
/// If an implementation implements multiple Service Interfaces, all service interfaces and the implementation will be provided, all referring to the same instance<br/>
/// If there are multiple implementations for the same service interface, the implementation with the <see cref="ServiceImplementationOverrideAttribute"/> will be used<br/>
/// It is highly recommended to call<see cref="IExceptionProvider.ThrowIfNotEmpty"/> before building the service provider.
/// see <see cref="ServiceImplementationOverrideAttribute"/> for more information on how to override implementations
/// </summary>
/// <remarks>
/// Examples:
/// <example>
/// <code>
/// // provided as singleton
/// [Service(ServiceLifetime.Singleton)]
/// class ShoppingService { }
/// 
/// // provided as singleton
/// [Service(ServiceLifetime.Singleton)]
/// class ShoppingService : IShoppingService { }
/// // ignored
/// interface IShoppingService { }
/// 
/// // not provided
/// class ShoppingService : IShoppingService { }
/// // provided as singleton with ShoppingService as implementation (type)
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingService { }
/// 
/// // provided as singleton
/// [Service(ServiceLifetime.Singleton)]
/// class ShoppingService : IShoppingService { }
/// // provided as singleton with ShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingService { }
/// 
/// // throws exception, since the implementation has a lower lifetime than the interface
/// [Service(ServiceLifetime.Scoped)]
/// class ShoppingService : IShoppingService { }
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingService { } 
/// 
/// // provided as singleton
/// [Service(ServiceLifetime.Singleton)]
/// class ShoppingService : IShoppingService { }
/// // provided as scoped with ShoppingService as alias factory - this means the service will act like a singleton but be injected as scoped
/// [Service(ServiceLifetime.Scoped)]
/// interface IShoppingService { }
/// 
/// // provided as singleton
/// class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// // provided as singleton with ShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingQueryService { }
/// // provided as scoped with ShoppingService as alias factory
/// [Service(ServiceLifetime.Scoped)]
/// interface IShoppingCommandService { }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class ServiceAttribute : Attribute
{

    /// <summary>
    /// Gets the lifetime of the service.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceAttribute"/> class with the specified service lifetime.
    /// </summary>
    /// <param name="serviceLifetime">The lifetime of the service.</param>
    public ServiceAttribute(ServiceLifetime serviceLifetime)
    {
        ServiceLifetime = serviceLifetime;
    }
}

/// <summary>
/// Registers to override the given type (<see cref="MatchServiceInterfacesOfType"/>)
/// used by <see cref="AttributeServiceCollectionExtension.AddServicesWithAttributes"/>
/// adds types with the <see cref="ServiceAttribute"/> to the service collection.<br/>
/// if it is a class, it will be treated as implementation and will always be provided with the <see cref="ServiceLifetime"/> of <see cref="ServiceAttribute.ServiceLifetime"/><br/>
/// if it is an interface, each class implementing this interface will be added.<br/>
/// If the implementation has no <see cref="ServiceAttribute"/>, the implementation will be provided with the <see cref="ServiceLifetime"/> of the interface.<br/>
/// If an implementation implements multiple Service Interfaces, all service interfaces and the implementation will be provided, all referring to the same instance<br/>
/// If there are multiple implementations for the same service interface, the implementation with the <see cref="ServiceImplementationOverrideAttribute"/> will be used<br/>
/// It is highly recommended to call<see cref="IExceptionProvider.ThrowIfNotEmpty"/> before building the service provider.
/// </summary>
/// <remarks>
/// Examples:
/// <example>
/// <code>
///
/// undesirable:
/// // provided as singleton
/// class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// // provided as singleton
/// class MockShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// // provided twice as singleton with ShoppingService as alias factory and MockShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingQueryService { }
/// // provided twice as singleton with ShoppingService as alias factory and MockShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingCommandService { }
/// 
///  solution:
/// // provided as singleton
/// class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// // provided as singleton
/// [ServiceImplementationOverride] // add this attribute to define implementation priority
/// class MockShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// // provided as singleton with MockShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingQueryService { }
/// // provided as singleton with MockShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingCommandService { }
/// 
/// undesirable:
/// // provided as singleton
/// class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// // provided as singleton
/// [ServiceImplementationOverride]
/// class MockShoppingService : IShoppingCommandService { }
/// // provided as singleton with ShoppingService as alias factory - the non-mock implementation will be used since the mock does not implement this interface
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingQueryService { }
/// // provided as singleton with MockShoppingService as alias factory
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingCommandService { }
/// 
/// solution:
/// class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// [ServiceImplementationOverride(typeof(ShoppingService))] // causes an exception to be thrown indicating the missing interface
/// class MockShoppingService : IShoppingQueryService, IShoppingCommandService { }
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingQueryService { }
/// [Service(ServiceLifetime.Singleton)]
/// interface IShoppingCommandService { }
/// 
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceImplementationOverrideAttribute : Attribute
{
    /// <summary>
    /// the type (class) which should be overridden by this implementation
    /// </summary>
    public Type? MatchServiceInterfacesOfType { get; }
    /// <summary>
    /// when a service has multiple implementations, the implementation with this attribute will be used
    /// </summary>
    /// <param name="matchServiceInterfacesOfType">
    ///     an exception will be thrown, if the given type has other service interfaces than the decorated one<br/>
    ///     a service interface is an interface which is decorated with the <see cref="ServiceAttribute"/>
    /// </param>
    public ServiceImplementationOverrideAttribute(Type? matchServiceInterfacesOfType = null)
    {
        MatchServiceInterfacesOfType = matchServiceInterfacesOfType;
        if (matchServiceInterfacesOfType?.IsClass == false)
            throw new InvalidSetupException(
                $"the given {nameof(matchServiceInterfacesOfType)} {matchServiceInterfacesOfType.FullName()} is not a class and therefore not a valid implementation");
    }
}