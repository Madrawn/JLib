using System.Reflection;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using JLib.Data;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Helper;

[TvtFactoryAttribute.IsClass, TvtFactoryAttribute.HasAttribute(typeof(ExtendObjectTypeAttribute))]
public record TypeExtensionType(Type Value) : TypeValueType(Value);

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
    public static IRequestExecutorBuilder RegisterDataProvider(
        this IRequestExecutorBuilder builder, ServiceKind serviceKind = ServiceKind.Default)
    {
        foreach (var dataProviderRType in builder.Services
                     .Select(x=>x.ServiceType)
                     .Where(x=>x.ImplementsAny<IDataProviderR<IDataObject>>())
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
    public static IRequestExecutorBuilder RegisterService(
        this IRequestExecutorBuilder builder, Type service, ServiceKind serviceKind = ServiceKind.Default)
    {
        RegisterServiceMethod.MakeGenericMethod(service).Invoke(null, new object[] { builder, serviceKind });
        return builder;
    }
}