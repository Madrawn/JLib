using System.Reflection;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using JLib.DataProvider;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Helper;

/// <summary>
/// a type which extends a given graphql object.
/// </summary>
/// <seealso cref="AttributeTypeExtensionType"/>
/// <seealso cref="ClassTypeExtensionType"/>
public abstract record TypeExtensionType : TypeValueType
{
    /// <summary>
    /// <inheritdoc cref="TypeExtensionType"/>
    /// </summary>
    /// <param name="Value"></param>
    internal TypeExtensionType(Type Value) : base(Value)
    {
    }
}
/// <summary>
/// a type which extends a given graphql object.
/// </summary>
[TvtFactoryAttribute.IsClass, TvtFactoryAttribute.HasAttribute(typeof(ExtendObjectTypeAttribute))]
public record AttributeTypeExtensionType(Type Value) : TypeExtensionType(Value);


/// <summary>
/// a type which extends a given graphql object.
/// </summary>
[TvtFactoryAttribute.IsClass, TvtFactoryAttribute.IsDerivedFrom(typeof(ObjectTypeExtension))]
public record ClassTypeExtensionType(Type Value) : TypeExtensionType(Value);

/// <summary>
/// extension methods for the <seealso cref="IRequestExecutorBuilder"/>
/// </summary>
public static class RequestExecutorBuilderHelper
{

    /// <summary>
    /// <b>WARNING!</b> This method must be called after <b>ALL</b> DataProvider have been registered!
    /// </summary>
    public static IRequestExecutorBuilder AddTypeExtensions(
        this IRequestExecutorBuilder builder, ITypeCache typeCache)
    {
        foreach (var type in typeCache.All<TypeExtensionType>())
            builder.AddTypeExtension(type.Value);
        return builder;
    }

    /// <summary>
    /// <b>WARNING!</b> This method must be called after <b>ALL</b> DataProvider have been registered!
    /// </summary>
    public static IRequestExecutorBuilder RegisterDataProvider(this IRequestExecutorBuilder builder)
    {
        foreach (var dataProviderRType in builder.Services
                     .Select(x => x.ServiceType)
                     .Where(x => x.ImplementsAny<IDataProviderR<IDataObject>>())
                     .ToArray())
            builder.RegisterService(dataProviderRType);
        foreach (var dataProviderRwType in builder.Services
                     .Select(x => x.ServiceType)
                     .Where(x => x.ImplementsAny<IDataProviderRw<IEntity>>())
                     .ToArray())
            builder.RegisterService(dataProviderRwType);
        return builder;
    }

    private static readonly MethodInfo RegisterServiceMethod =
        typeof(RequestExecutorBuilderExtensions)
            .GetMethod(nameof(RequestExecutorBuilderExtensions.RegisterService))
        ?? throw new InvalidSetupException(
            $"{nameof(IRequestExecutorBuilder)}.{nameof(RequestExecutorBuilderExtensions.RegisterService)}Method Info not found");
    /// <summary>
    /// <inheritdoc cref="RequestExecutorBuilderExtensions.RegisterService{TService}"/>
    /// </summary>
    /// <param name="builder">The <see cref="IRequestExecutorBuilder"/>.</param>
    /// <param name="service">The service type.</param>
    /// <param name="serviceKind">The service kind defines the way a service is injected and handled by the execution engine.</param>
    /// <returns>
    /// An <see cref="IRequestExecutorBuilder"/> that can be used to configure a schema
    /// and its execution.
    /// </returns>
    public static IRequestExecutorBuilder RegisterService(
        this IRequestExecutorBuilder builder, Type service, ServiceKind serviceKind = ServiceKind.Default)
    {
        RegisterServiceMethod.MakeGenericMethod(service).Invoke(null, new object[] { builder, serviceKind });
        return builder;
    }
}