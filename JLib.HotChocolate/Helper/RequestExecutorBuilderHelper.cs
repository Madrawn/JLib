using System.Reflection;
using HotChocolate;
using HotChocolate.Execution.Configuration;
using JLib.Data;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Helper;

public static class RequestExecutorBuilderHelper
{
    public static IRequestExecutorBuilder RegisterDataProvider<TTvt>(
        this IRequestExecutorBuilder builder, ITypeCache typeCache, ServiceKind serviceKind = ServiceKind.Default)
        where TTvt : TypeValueType
    {
        foreach (var tvt in typeCache.All<TTvt>())
            builder.RegisterService(typeof(IDataProviderR<>).MakeGenericType(tvt.Value), serviceKind);
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