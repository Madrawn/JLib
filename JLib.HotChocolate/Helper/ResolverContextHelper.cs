using System.Linq.Expressions;

using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace JLib.Helper;

public static class ResolverContextHelper
{
    public static async Task<ICollection<TRes>> LoadMany<TId, TMqe, TRes>(
       this IResolverContext context,
       TId id,
       KeySelectorExpression<TMqe, TId> foreignIdSelector,
       Expression<Func<TMqe, bool>>? filter,
       Func<TMqe, TRes> resultSelector,
       CancellationToken token)
       where TMqe : IModuleQueryEntity
       where TId : struct
       => (await context.GroupDataLoader<TId, TMqe>(async (keys, token2) =>
       {
           using var scope = context.Services.CreateScope();
           return await scope.ServiceProvider.GetRequiredService<IDataProvider<TMqe>>()
               .QueryData()
               .Where(filter ?? ((TMqe _) => true))
               .ToLookupAsync(keys, foreignIdSelector, token2);
       },
           LoadManyIdentifier(foreignIdSelector)
       ).LoadAsync(id, token)).Select(resultSelector).ToList();

    #region overloads

    public static async Task<ICollection<TRes>> LoadManyTransformed<TMqe, TRes>(
        this IResolverContext context,
        int id,
        Expression<Func<TMqe, int?>> foreignIdSelector,
        Func<TMqe, TRes> resultSelector,
        CancellationToken token)
        where TMqe : IModuleQueryEntity
        => await context.LoadMany(id, foreignIdSelector, null, resultSelector, token);
    #endregion    

    public static async Task<ICollection<TGdo>> LoadMany<TId, TMqe, TGdo>(
        this IResolverContext context,
        TId id,
        KeySelectorExpression<TGdo, TId> foreignIdSelector,
        Expression<Func<TMqe, bool>>? filter)
        where TGdo : IGraphQlDataObject<TMqe>
        where TMqe : IModuleQueryEntity
        where TId : struct
        => await context.GroupDataLoader<TId, TGdo>(async (keys, token2) =>
            {
                using var scope = context.Services.CreateScope();

                return await scope.ServiceProvider.GetRequiredService<IDataProvider<TMqe>>()
                    .QueryData()
                    .Where(filter ?? ((TMqe _) => true))
                    .ProjectTo<TGdo>(context.Service<IConfigurationProvider>())
                    .ToLookupAsync(keys, foreignIdSelector, token2);
            },
            LoadManyIdentifier<TId, TMqe, TGdo>(foreignIdSelector)
        ).LoadAsync(id, context.RequestAborted);

    #region overloads

    public static async Task<ICollection<TGdo>> LoadMany<TMqe, TGdo>(
       this IResolverContext context,
       int id,
       Expression<Func<TGdo, int?>> foreignIdSelector)
       where TGdo : IGraphQlDataObject<TMqe>
       where TMqe : IModuleQueryEntity
       => await context.LoadMany<int, TMqe, TGdo>(id, foreignIdSelector, null);

    public static async Task<ICollection<TGdo>> LoadMany<TMqe, TGdo>(
        this IResolverContext context,
        int id,
        Expression<Func<TGdo, int?>> foreignIdSelector,
        Expression<Func<TMqe, bool>>? filter,
        CancellationToken token)
        where TGdo : IGraphQlDataObject<TMqe>
        where TMqe : IModuleQueryEntity
        => await context.LoadMany<int, TMqe, TGdo>(id, foreignIdSelector, filter);

    #endregion

    public static async Task<TGdo> LoadOne<TId, TMqe, TGdo>(
        this IResolverContext context,
        TId id,
        KeySelectorExpression<TGdo, TId> idSelector,
        Expression<Func<TMqe, bool>>? filter)
        where TGdo : IGraphQlDataObject<TMqe>
        where TMqe : IModuleQueryEntity
        where TId : struct
        => await context.BatchDataLoader<TId, TGdo>(async (keys, token2) =>
            {
                using var scope = context.Services.CreateScope();
                var provider = scope.ServiceProvider.GetRequiredService<IDataProvider<TMqe>>();
                return await provider
                    .QueryData()
                    .Where(filter ?? ((TMqe _) => true))
                    .ProjectTo<TGdo>(context.Service<IConfigurationProvider>())
                    .ToReadOnlyDictionaryAsync(keys, idSelector, token2);
            },
            LoadOneIdentifier(idSelector)
        ).LoadAsync(id, context.RequestAborted);

    #region overloads

    public static async Task<TGdo?> LoadOne<TMqe, TGdo>(
        this IResolverContext context,
        int? id)
        where TGdo : class, IGraphQlDataObject<TMqe>, IIdentifiableObject<int>
        where TMqe : IModuleQueryEntity
        => id.HasValue
            ? (await context.LoadOne<int, TMqe, TGdo>(id.Value, new(gdo => gdo.Id), null))
            : null;

    public static async Task<TGdo?> LoadOne<TMqe, TGdo>(
       this IResolverContext context,
       int? id,
       Expression<Func<TMqe, bool>> filter)
       where TGdo : class, IGraphQlDataObject<TMqe>, IIdentifiableObject<int>
       where TMqe : IModuleQueryEntity
       => id.HasValue
           ? (await context.LoadOne<int, TMqe, TGdo>(id.Value, new(gdo => gdo.Id), filter))
           : null;

    public static async Task<TGdo?> LoadOne<TMqe, TGdo>(
        this IResolverContext context,
        int? id,
        Expression<Func<TGdo, int>> keySelector,
        Expression<Func<TMqe, bool>> filter)
        where TGdo : class, IGraphQlDataObject<TMqe>
        where TMqe : IModuleQueryEntity
        => id.HasValue
            ? (await context.LoadOne<int, TMqe, TGdo>(id.Value, keySelector, filter))
            : null;

    #endregion

    [UseFiltering, UseSorting]
    public static async Task<ICollection<TChildGdo>> LoadMany<TParentId, TChildId, TLinkMqe, TChildMqe, TChildGdo>(
        this IResolverContext context,
        TParentId parentId,
        KeySelectorExpression<TLinkMqe, TParentId> linkToParentIdSelector,
        KeySelectorExpression<TLinkMqe, TChildId> linkToChildIdSelector,
        CancellationToken token)
        where TParentId : struct
        where TChildId : struct
        where TLinkMqe : IModuleQueryEntity
        where TChildMqe : IModuleQueryEntity, IIdentifiableObject<TChildId>
        where TChildGdo : IGraphQlDataObject<TChildMqe>, IIdentifiableObject<TChildId>
    {
        return (await context.GroupDataLoader<TParentId, JoinContainer<TParentId, TChildGdo>>(
                    async (keys, token2) =>
                    {
                        using var scope = context.Services.CreateScope();
                        return await scope.ServiceProvider.GetRequiredService<IDataProvider<TLinkMqe>>()
                            .QueryData()
                            .Join(
                                scope.ServiceProvider.GetRequiredService<IDataProvider<TChildMqe>>()
                                    .QueryData()
                                    .ProjectTo<TChildGdo>(context.Service<IConfigurationProvider>())
                                    .AsQueryable(),
                                linkToChildIdSelector.NullableExpression,
                                child => child.Id,
                                GetJoinContainerFactory<TParentId, TLinkMqe, TChildGdo>(linkToParentIdSelector)
                            ).ToLookupAsync(keys, new(container => container.ParentId), token2);
                    },
                    LoadManyIdentifier<TParentId, TChildId, TLinkMqe, TChildMqe>(linkToParentIdSelector, linkToChildIdSelector)
                ).LoadAsync(parentId, token)
            ).Select(container => container.Child)
            .ToList();
    }

    #region overloads

    public static async Task<ICollection<TChildGdo>> LoadMany<TLinkMqe, TChildMqe, TChildGdo>(
        this IResolverContext context,
        int parentId,
        Expression<Func<TLinkMqe, int>> linkToParentIdSelector,
        Expression<Func<TLinkMqe, int>> linkToChildIdSelector,
        CancellationToken token)
        where TLinkMqe : IModuleQueryEntity
        where TChildMqe : IModuleQueryEntity, IIdentifiableObject<int>
        where TChildGdo : IGraphQlDataObject<TChildMqe>, IIdentifiableObject<int>
        => await context.LoadMany<int, int, TLinkMqe, TChildMqe, TChildGdo>(parentId, linkToParentIdSelector, linkToChildIdSelector, token);

    #endregion

    [UseFiltering, UseSorting]
    public static async Task<TChildGdo> LoadOne<TLinkId, TChildId, TLinkMqe, TChildMqe, TChildGdo>(
       this IResolverContext context,
       TLinkId parentId,
       Expression<Func<TLinkMqe, TChildId>> linkToChildIdSelector)
       where TLinkId : struct
       where TChildId : struct
       where TLinkMqe : IModuleQueryEntity, IIdentifiableObject<TLinkId>
       where TChildMqe : IModuleQueryEntity, IIdentifiableObject<TChildId>
       where TChildGdo : IGraphQlDataObject<TChildMqe>, IIdentifiableObject<TChildId>
    {
        return (await context.BatchDataLoader<TLinkId, JoinContainer<TLinkId, TChildGdo>>(
            async (keys, token2) =>
            {
                using var scope = context.Services.CreateScope();
                return await scope.ServiceProvider.GetRequiredService<IDataProvider<TLinkMqe>>()
                    .QueryData()
                    .Join(
                        scope.ServiceProvider.GetRequiredService<IDataProvider<TChildMqe>>()
                            .QueryData()
                            .ProjectTo<TChildGdo>(context.Service<IConfigurationProvider>())
                            .AsQueryable(),
                        linkToChildIdSelector,
                        child => child.Id,
                        GetJoinContainerFactory<TLinkId, TLinkMqe, TChildGdo>(new(link => link.Id))
                    ).ToReadOnlyDictionaryAsync(keys, new(container => container.ParentId), token2);
            },
            LoadOneIdentifier<TLinkId, TChildId, TLinkMqe, TChildGdo>(new(x => x.Id), linkToChildIdSelector)
            ).LoadAsync(parentId, context.RequestAborted)
        ).Child;
    }

    #region overloads

    public static async Task<TChildGdo> LoadOne<TLinkMqe, TChildMqe, TChildGdo>(
        this IResolverContext context,
        int parentId,
        Expression<Func<TLinkMqe, int>> linkToChildIdSelector)
        where TLinkMqe : IModuleQueryEntity, IIdentifiableObject<int>
        where TChildMqe : IModuleQueryEntity, IIdentifiableObject<int>
        where TChildGdo : IGraphQlDataObject<TChildMqe>, IIdentifiableObject<int>
        => await context.LoadOne<int, int, TLinkMqe, TChildMqe, TChildGdo>(parentId, linkToChildIdSelector);

    public static async Task<TChildGdo?> LoadOne<TLinkMqe, TChildMqe, TChildGdo>(
        this IResolverContext context,
        int? parentId,
        Expression<Func<TLinkMqe, int>> linkToChildIdSelector)
        where TLinkMqe : IModuleQueryEntity, IIdentifiableObject<int>
        where TChildMqe : IModuleQueryEntity, IIdentifiableObject<int>
        where TChildGdo : class, IGraphQlDataObject<TChildMqe>, IIdentifiableObject<int>
        => parentId.HasValue
            ? (await context.LoadOne<int, int, TLinkMqe, TChildMqe, TChildGdo>(parentId.Value, linkToChildIdSelector))
            : null;

    #endregion

    #region Identifier builder

    public static string LoadManyIdentifier<TParentId, TChildId, TLink, TChild>(
        KeySelectorExpression<TLink, TParentId> parentId, KeySelectorExpression<TLink, TChildId> childId)
        where TParentId : struct
        where TChildId : struct
        => $"?<=[{parentId.PropertyName}]=N {typeof(TLink).Name}=[{childId.PropertyName}]=>N {typeof(TChild).Name}";

    public static string LoadManyIdentifier<TId, TChildMqe>(KeySelectorExpression<TChildMqe, TId> id)
        where TId : struct
        where TChildMqe : IModuleQueryEntity
        => LoadManyIdentifier<TId, TChildMqe, TChildMqe>(id);
    public static string LoadManyIdentifier<TId, TChildMqe, TJoin>(KeySelectorExpression<TJoin, TId> id)
        where TId : struct
        where TChildMqe : IModuleQueryEntity
        => $"{typeof(TChildMqe).Name}=[{id.PropertyName}]=>1 {typeof(TChildMqe).Name}";
    public static string LoadOneIdentifier<TId, TChild>(KeySelectorExpression<TChild, TId> id)
        where TId : struct
        => $"?<=[{id.PropertyName}]=N {typeof(TChild).Name}";
    public static string LoadOneIdentifier<TLinkId, TChildId, TLink, TChild>(
        KeySelectorExpression<TLink, TLinkId> linkId, KeySelectorExpression<TLink, TChildId> childId)
        where TLinkId : struct
        where TChildId : struct
        => $"?=[{linkId.PropertyName}]=>1 {typeof(TLink).Name}=[{childId.PropertyName}]=>1 {typeof(TChild).Name}";

    #endregion

    #region join container helper
    struct JoinContainer<TParentId, TChildGdo>
        where TParentId : notnull
        where TChildGdo : IGraphQlDataObject
    {
        public TParentId ParentId { get; init; }

        public TChildGdo Child { get; init; }

    }

    static Expression<Func<TLinkMqe, TChildGdo, JoinContainer<TParentId, TChildGdo>>> GetJoinContainerFactory<TParentId, TLinkMqe, TChildGdo>(
        KeySelectorExpression<TLinkMqe, TParentId> linkToParentIdSelector)
        where TParentId : struct
        where TLinkMqe : IModuleQueryEntity
        where TChildGdo : IGraphQlDataObject
    {
        // Create two parameters for the Func, one for the TLinkMqe object and one for the TChildGdo object
        var linkParameter = Expression.Parameter(typeof(TLinkMqe), "link");
        var childParameter = Expression.Parameter(typeof(TChildGdo), "child");

        // Create a property for the ParentId of the TLinkMqe object by applying the linkToParentIdSelector expression to the linkParameter object
        var parentIdProperty = Expression.Invoke(linkToParentIdSelector.LambdaExpression, linkParameter);

        // Create a new Container instance
        var container = Expression.New(typeof(JoinContainer<TParentId, TChildGdo>));

        // Create an array of MemberBindings to initialize the properties of the Container instance
        var containerBindings = new MemberBinding[]
        {
            Expression.Bind(typeof(JoinContainer<TParentId, TChildGdo>).GetProperty(nameof(JoinContainer<TParentId, TChildGdo>.ParentId))!, parentIdProperty),
            Expression.Bind(typeof(JoinContainer<TParentId, TChildGdo>).GetProperty(nameof(JoinContainer<TParentId, TChildGdo>.Child))!, childParameter),
        };

        // Create a MemberInitExpression that returns the Container instance with the initialized properties
        var containerMemberInit = Expression.MemberInit(container, containerBindings);

        // Create a LambdaExpression that returns the Func that creates the Container instance and initializes the properties
        return Expression.Lambda<Func<TLinkMqe, TChildGdo, JoinContainer<TParentId, TChildGdo>>>(containerMemberInit, linkParameter, childParameter);
    }
    #endregion
}
