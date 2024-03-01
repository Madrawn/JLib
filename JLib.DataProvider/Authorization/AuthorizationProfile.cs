using System.Linq.Expressions;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataProvider.Authorization;

[NotAbstract, IsDerivedFrom(typeof(AuthorizationProfile))]
public record AuthorizationProfileType(Type Value) : TypeValueType(Value), IPostNavigationInitializedType
{
    public AuthorizationProfile Instance { get; private set; } = null!;

    public void Initialize(ITypeCache cache, ExceptionBuilder exceptions)
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

    /// <summary>
    /// adds a authorization rule for all <typeparamref name="TDataObject"/>s
    /// <br/>the <paramref name="queryPredicate"/> and <paramref name="dataObjectPredicate"/> are applied as is
    /// </summary>
    /// <typeparam name="TDataObject">the dataObject to be authorized</typeparam>
    /// <typeparam name="TS1">the first service</typeparam>
    /// <param name="queryPredicate">used to authorize queries</param>
    /// <param name="dataObjectPredicate">used to authorize materialized dataObjects</param>
    protected void AddAuthorization<TDataObject, TS1>(
        Func<TS1, Expression<Func<TDataObject, bool>>> queryPredicate,
        Func<TS1, TDataObject, bool> dataObjectPredicate
    )
        where TDataObject : class, IDataObject
        where TS1 : notnull
        => _authorizationProvider.Add(
            new UnboundAuthorizationInfo<TDataObject, TS1>(queryPredicate, dataObjectPredicate, _typeCache));

    /// <summary>
    /// adds a authorization rule for all <typeparamref name="TDataObject"/>
    /// the <paramref name="predicate"/> will be
    /// - once compiled and used for data object Authorization
    /// - visited so that the service parameter becomes a constant. this happens each time a query is authorized
    /// <br/>
    /// </summary>
    /// <typeparam name="TDataObject">the dataObject to be authorized</typeparam>
    /// <typeparam name="TS1">the first service</typeparam>
    /// <param name="predicate">the authorization rule for this dataObject</param>
    protected void AddAuthorization<TDataObject, TS1>(
        Expression<Func<TS1, TDataObject, bool>> predicate
    )
        where TDataObject : class, IDataObject
        where TS1 : notnull
    {
        var p1 = predicate.Parameters[0];
        var p2 = predicate.Parameters[1];


        _authorizationProvider.Add(new UnboundAuthorizationInfo<TDataObject, TS1>(ExpressionGenerator,
            predicate.Compile(), _typeCache));

        return;

        Expression<Func<TDataObject, bool>> ExpressionGenerator(TS1 service)
        {
            var c = Expression.Constant(service, typeof(TS1));
            var body = new ParToConstVisitor(p1, c).Visit(predicate.Body);
            return Expression.Lambda<Func<TDataObject, bool>>(body, p2);
        }
    }

    #endregion
}

internal class ParToConstVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _par;
    private readonly ConstantExpression _const;

    public ParToConstVisitor(ParameterExpression par, ConstantExpression @const)
    {
        _par = par;
        _const = @const;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _par
            ? _const
            : base.VisitParameter(node);
    }
}