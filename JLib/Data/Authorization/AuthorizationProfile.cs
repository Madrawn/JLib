using System.Linq.Expressions;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Data.Authorization;

[NotAbstract, IsDerivedFrom(typeof(AuthorizationProfile))]
public record AuthorizationProfileType(Type Value) : TypeValueType(Value), IPostNavigationInitializedType
{
    public AuthorizationProfile Instance { get; private set; } = null!;
    public void Initialize(ITypeCache cache, IExceptionManager exceptions)
    {
        Instance = Activator.CreateInstance(Value, new object[] { cache })?.As<AuthorizationProfile>()
                   ?? throw new("instance could not be created");
    }
}
/// <summary>
/// can be derived from to add unboundAuthorization to an entity
/// <br/> the resulting unboundAuthorization can be applied using the <see cref="IAuthorizationManager"/>
/// </summary>
public abstract class AuthorizationProfile
{
    private readonly ITypeCache _typeCache;
    private readonly List<IUnboundAuthorizationInfo> _authorizationProvider = new();
    internal IReadOnlyCollection<IUnboundAuthorizationInfo> Build()
        => _authorizationProvider;

    protected AuthorizationProfile(ITypeCache typeCache)
    {
        _typeCache = typeCache;
    }
    #region AddAuthorization

    protected void AddAuthorization<TDataObject, TS1>(
        Func<TS1, Expression<Func<TDataObject, bool>>> authorizationQueryablePredicate,
        Func<TS1, TDataObject, bool> authorizationPredicate
    )
        where TDataObject : class, IDataObject
        where TS1 : notnull
        => _authorizationProvider.Add(new UnboundAuthorizationInfo<TDataObject, TS1>(authorizationQueryablePredicate, authorizationPredicate, _typeCache));

    #endregion


}