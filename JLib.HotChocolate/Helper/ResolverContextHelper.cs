using System.Reflection;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JLib.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.HotChocolate.Helper;

public static class ResolverContextHelper
{
    /// <summary>
    /// fetched the given dataObject via <see cref="IDataProviderR{TData}"/>, creates a BatchDataLoader using the primary key and retrieves the data of the given id.
    /// <br/>uses the EfCore specific ToDictionaryAsync method. Not Compatible with other DataProviders.
    /// <br/>creates a new scope to prevent the disposed exception of circular batchDataLoader calls
    /// </summary>
    public static async Task<TDo> GetOneDataObjectAsync<TDo>(this IResolverContext context, Guid id)
        where TDo : class, IDataObject
        => (await context.GetOneDataObjectAsync<TDo>((Guid?)id))!;
    /// <summary>
    /// <inheritdoc cref="GetOneDataObjectAsync{TDo}(IResolverContext,Guid)"/>
    /// </summary>
    public static async Task<TDo?> GetOneDataObjectAsync<TDo>(this IResolverContext context, Guid? id)
        where TDo : class, IDataObject
        => id.HasValue
            ? await context.BatchAsync<Guid, TDo>(
                async (ids, token)
                    =>
                {
                    using var scope = context.Services.CreateScope();
                    return await scope.ServiceProvider.GetRequiredService<IDataProviderR<TDo>>()
                        .Get().Where(dataObject => ids.Contains(dataObject.Id))
                        .ToDictionaryAsync(gdo => gdo.Id, token);
                },
                id.Value)
            : null;    
}