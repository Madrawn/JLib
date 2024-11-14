using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.SystemTextJson;

/// <summary>
/// Defines the Order, in which the converters discovered by <seealso cref="JsonConverterCollectionExtensions"/> will be added. The Default value is 0.
/// </summary>
public sealed class ConverterOrderAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public const double DefaultOrder = 0;
    /// <summary>
    /// 
    /// </summary>
    public double Order { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="order"></param>
    public ConverterOrderAttribute(double order) => Order = order;
}

/// <summary>
/// utility methods for managing <see cref="JsonConverter"/>s
/// </summary>
public static class JsonConverterCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="JsonConverterType"/>s from the specified <paramref name="typeCache"/> to the collection.<br/>
    /// The Constructor must either have no arguments or receive a single <see cref="ITypeCache"/>, otherwise the converter will not be added.<br/>
    /// <seealso cref="ConverterOrderAttribute"/>
    /// </summary>
    /// <param name="collection">The collection of <see cref="JsonConverter"/>.</param>
    /// <param name="typeCache">The <see cref="ITypeCache"/> containing the converter types.</param>
    public static void AddConverters(this IList<JsonConverter> collection, ITypeCache typeCache)
    {
        foreach (var converter in typeCache.All<JsonConverterType>()
                     .Select(c => c.Create(typeCache))
                     .WhereNotNull()
                     .OrderBy(t => t.GetType().GetCustomAttribute<ConverterOrderAttribute>()?.Order ?? 0)
                     )
            collection.Add(converter);
    }


    /// <summary>
    /// Adds the <see cref="JsonConverterType"/>s from the specified <paramref name="typeCache"/> to the collection.<br/>
    /// The Constructor must either have no arguments or receive a single <see cref="ITypeCache"/>, otherwise the converter will not be added.<br/>
    /// <seealso cref="ConverterOrderAttribute"/>
    /// </summary>
    public static IServiceCollection AddJsonConverters(this IServiceCollection services, ITypeCache typeCache)
        => services.Configure<JsonSerializerOptions>(o =>
            o.Converters.AddConverters(typeCache));
}
