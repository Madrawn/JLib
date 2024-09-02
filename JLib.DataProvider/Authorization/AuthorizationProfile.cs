using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using static JLib.Reflection.TvtFactoryAttribute;
using ValueType = JLib.ValueTypes.ValueType;

namespace JLib.DataProvider.Authorization;

/// <summary>
/// Represents a non-abstract type that is derived from <see cref="AuthorizationProfile"/>
/// </summary>
/// <param name="Value"></param>
[NotAbstract, IsDerivedFrom(typeof(AuthorizationProfile))]
public record AuthorizationProfileType(Type Value) : TypeValueType(Value), IPostNavigationInitializedType
{
    /// <summary>
    /// the singleton instance of this <see cref="Value"/>
    /// </summary>
    public AuthorizationProfile Instance { get; private set; } = null!;

    /// <summary>
    /// Internal use only. Initializes the instance of the AuthorizationProfile
    /// </summary>
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

    /// <summary>
    /// the types that are authorized by this profile
    /// </summary>
    protected IEnumerable<DataObjectType> AuthorizedTypes => _authorizationProvider.Select(x => x.Target);

    internal IReadOnlyCollection<IUnboundAuthorizationInfo> Build()
        => _authorizationProvider;

    /// <summary>
    /// Creates a new instance of <see cref="AuthorizationProfile"/>
    /// </summary>
    /// <param name="typeCache"></param>
    protected AuthorizationProfile(ITypeCache typeCache)
    {
        _typeCache = typeCache;
    }

    #region AddGenericAuthorization

    /// <summary>
    /// adds an authorization rule for all <typeparamref name="TDataObject"/>s
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
    /// adds an authorization rule for all <typeparamref name="TDataObject"/>
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
        => _authorizationProvider.Add(new UnboundAuthorizationInfo<TDataObject, TS1>(service => TransformExpressionToNestedFunc(service, predicate),
            predicate.Compile(), _typeCache));

    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <param name="dataObjectsToBeAuthorized">An Array of <see cref="Type"/>s which are assignable to <typeparamref name="TShared"/> and will be authorized using the <paramref name="predicate"/></param>
    protected void AddGenericAuthorization<TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate, params Type[] dataObjectsToBeAuthorized)
    {
        dataObjectsToBeAuthorized.Where(x => x.IsAssignableTo(typeof(TShared)) is false).Select(x =>
            new DataObjectNotAssignableToSharedTypeException(x, typeof(TShared)))
            .ThrowExceptionIfNotEmpty($"Some {nameof(dataObjectsToBeAuthorized)} are not assignable to {typeof(TShared).FullName}");

        foreach (var dataObjectType in dataObjectsToBeAuthorized)
            AddAuthorizationMi.MakeGenericMethod(dataObjectType, typeof(TShared)).Invoke(this, new object[] { predicate });
    }

    #region multi data object
    private static readonly MethodInfo AddAuthorizationMi = typeof(AuthorizationProfile)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        .SingleOrDefault(mi =>
        {
            var parameters = mi.GetParameters();
            var typeArguments = mi.TryGetGenericArguments();
            return mi is
            {
                Name: nameof(AddGenericAuthorization),
                IsGenericMethodDefinition: true
            }
                   && typeArguments.Length == 2
                   && parameters.Length == 1
                   && parameters.Single()
                       .ParameterType ==
                           //Expression<Func<IServiceProvider, TShared, bool>>
                           typeof(Expression<>)
                               .MakeGenericType(typeof(Func<,,>)
                                   .MakeGenericType(
                                       typeof(IServiceProvider),
                                       typeArguments[1],
                                       typeof(bool)
                                   )
                               );
        })
        ?? throw new InvalidSetupException(nameof(AddAuthorization) + " Method Info could not be found");
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TDataObject3, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
        where TDataObject3 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject3, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TDataObject3, TDataObject4, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
        where TDataObject3 : class, TShared, IDataObject
        where TDataObject4 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject3, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject4, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TDataObject3, TDataObject4, TDataObject5, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
        where TDataObject3 : class, TShared, IDataObject
        where TDataObject4 : class, TShared, IDataObject
        where TDataObject5 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject3, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject4, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject5, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TDataObject3, TDataObject4, TDataObject5, TDataObject6, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
        where TDataObject3 : class, TShared, IDataObject
        where TDataObject4 : class, TShared, IDataObject
        where TDataObject5 : class, TShared, IDataObject
        where TDataObject6 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject3, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject4, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject5, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject6, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TDataObject3, TDataObject4, TDataObject5, TDataObject6, TDataObject7, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
        where TDataObject3 : class, TShared, IDataObject
        where TDataObject4 : class, TShared, IDataObject
        where TDataObject5 : class, TShared, IDataObject
        where TDataObject6 : class, TShared, IDataObject
        where TDataObject7 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject3, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject4, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject5, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject6, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject7, TShared, IServiceProvider>(predicate));
    }
    /// <summary>
    /// Adds an authorization rule for all passed DataObject type arguments using the common type <typeparamref name="TShared"/>
    /// </summary>
    /// <param name="predicate">an expression which will be used to authorize all passed DataObject type arguments</param>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TShared}(Expression{Func{IServiceProvider,TShared,bool}})"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TShared}"/>
    /// <seealso cref="AddGenericAuthorization{TDataObject1,TDataObject2,TDataObject3,TDataObject4,TDataObject5,TDataObject6,TDataObject7,TDataObject8,TShared}"/>
    protected void AddGenericAuthorization<TDataObject1, TDataObject2, TDataObject3, TDataObject4, TDataObject5, TDataObject6, TDataObject7, TDataObject8, TShared>(
        Expression<Func<IServiceProvider, TShared, bool>> predicate)
        where TDataObject1 : class, TShared, IDataObject
        where TDataObject2 : class, TShared, IDataObject
        where TDataObject3 : class, TShared, IDataObject
        where TDataObject4 : class, TShared, IDataObject
        where TDataObject5 : class, TShared, IDataObject
        where TDataObject6 : class, TShared, IDataObject
        where TDataObject7 : class, TShared, IDataObject
        where TDataObject8 : class, TShared, IDataObject
    {
        AddAuthorization(TransformSharedToExplicit<TDataObject1, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject2, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject3, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject4, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject5, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject6, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject7, TShared, IServiceProvider>(predicate));
        AddAuthorization(TransformSharedToExplicit<TDataObject8, TShared, IServiceProvider>(predicate));
    }
    #endregion
    private static Expression<Func<TService1, TDataObject, bool>> TransformSharedToExplicit<TDataObject, TShared, TService1>(Expression<Func<TService1, TShared, bool>> predicate)
    {
        var p1 = predicate.Parameters[0];
        var p2 = predicate.Parameters[1];

        var dataObjectPar = Expression.Parameter(typeof(TDataObject), p2.Name);
        var body = new ReplaceParVisitor2(p2, dataObjectPar).Visit(predicate.Body);
        return Expression.Lambda<Func<TService1, TDataObject, bool>>(body, p1, dataObjectPar);

    }


    private static Expression<Func<TDataObject, bool>> TransformExpressionToNestedFunc<TDataObject, TS1>(TS1 service, Expression<Func<TS1, TDataObject, bool>> predicate)
    {
        var p1 = predicate.Parameters[0];
        var p2 = predicate.Parameters[1];

        var c = Expression.Constant(service, typeof(TS1));
        var body = new ReplaceParVisitor(p1, c).Visit(predicate.Body);
        return Expression.Lambda<Func<TDataObject, bool>>(body, p2);
    }
    #endregion


    /// <summary>
    /// Checks whether this <see cref="AuthorizationProfile"/> contains Rules for all instances of <typeparamref name="TDataObjectType"/> contained in the current <see cref="ITypeCache"/>
    /// </summary>
    /// <typeparam name="TDataObjectType"></typeparam>
    /// <returns>an <see cref="IExceptionProvider"/> which contains an <see cref="Exception"/> for each <typeparamref name="TDataObjectType"/> that are not authorized within this <see cref="AuthorizationProfile"/></returns>
    public IExceptionProvider EnsureAuthorised<TDataObjectType>()
        where TDataObjectType : class, IDataObjectType
    {
        var exceptions = new ExceptionBuilder($"Some {typeof(TDataObjectType).FullName(true)} types are not authorized");

        if (_typeCache.KnownTypeValueTypes.Contains(typeof(TDataObjectType)) is false)
            exceptions.Add(new UnknownDataObjectTypeException<TDataObjectType>());

        exceptions.Add(
            _typeCache.All<TDataObjectType>()
                .Except(this.AuthorizedTypes.OfType<TDataObjectType>())
                .Select(tdo => new MissingAuthorizationException(tdo))
            );
        return exceptions;
    }

}

#region exceptions
/// <summary>
/// Indicates, that <see cref="TypeWithoutAuthorization"/> Is not authorized in a given <see cref="AuthorizationProfile"/> but should be.
/// </summary>
public class MissingAuthorizationException : JLibException
{
    /// <summary>
    /// the type which is not authorized
    /// </summary>
    public IDataObjectType TypeWithoutAuthorization { get; }

    internal MissingAuthorizationException(IDataObjectType typeWithoutAuthorization) : base($"{typeWithoutAuthorization.Value.FullName()} is not authorized.")
    {
        TypeWithoutAuthorization = typeWithoutAuthorization;
        Data.Add(nameof(TypeWithoutAuthorization), typeWithoutAuthorization);
    }
}

/// <summary>
/// Indicates, that the type parameter of <see cref="AuthorizationProfile.EnsureAuthorised{TDataObjectType}"/> is not known to the <see cref="ITypeCache"/>.
/// </summary>
public abstract class UnknownDataObjectTypeException : InvalidSetupException
{
    /// <summary>
    /// the type which is not known to the type cache
    /// </summary>
    public Type UnknownDataObjectType { get; }

    internal UnknownDataObjectTypeException(Type unknownDataObjectType) : base($"The {nameof(IDataObjectType)} {unknownDataObjectType.FullName()} is not known to the type cache")
    {
        UnknownDataObjectType = unknownDataObjectType;
        Data[nameof(UnknownDataObjectType)] = unknownDataObjectType;
    }
}

/// <summary>
/// <inheritdoc cref="UnknownDataObjectTypeException"/>
/// </summary>
public sealed class UnknownDataObjectTypeException<TDataObjectType> : UnknownDataObjectTypeException
    where TDataObjectType : class, IDataObjectType
{
    internal UnknownDataObjectTypeException() : base(typeof(IDataObjectType)) { }
}

/// <summary>
/// Indicates, that the type parameter of <see cref="AuthorizationProfile.EnsureAuthorised{TDataObjectType}"/> is not known to the <see cref="ITypeCache"/>.
/// </summary>
public abstract class InvalidDataObjectTypeException : InvalidSetupException
{
    /// <summary>
    /// the type which is not known to the type cache
    /// </summary>
    public Type InvalidDataObjectType { get; }

    internal InvalidDataObjectTypeException(Type invalidDataObjectType, string message) : base(message)
    {
        InvalidDataObjectType = invalidDataObjectType;
        Data[nameof(InvalidDataObjectType)] = invalidDataObjectType;
    }
}
/// <summary>
/// indicates, that a given <see cref="Expression"/> is not supported by the <see cref="AuthorizationProfile"/>
/// </summary>
public class DataObjectNotAssignableToSharedTypeException : InvalidDataObjectTypeException
{
    /// <summary>
    /// The Type <see cref="InvalidDataObjectTypeException.InvalidDataObjectType"/> mus be assignable to
    /// </summary>
    public Type SharedType { get; }
    internal DataObjectNotAssignableToSharedTypeException(Type invalidDataObjectType, Type sharedType) : base(invalidDataObjectType, $"The {invalidDataObjectType.FullName} is not assignable to {sharedType.FullName()}.")
    {
        SharedType = sharedType;
        Data[nameof(SharedType)] = sharedType;
    }
}

/// <summary>
/// indicates, that something happened during the <see cref="AuthorizationProfile"/> setup
/// </summary>
public class AuthorizationProfileException : JLibException
{
    internal AuthorizationProfileException(string message) : base(message)
    {

    }
}
/// <summary>
/// indicates, that a given <see cref="Expression"/> is not supported by the <see cref="AuthorizationProfile"/>
/// </summary>
public class UnsupportedExpressionException : AuthorizationProfileException
{
    /// <summary>
    /// the unsupported expression
    /// </summary>
    public Expression Expression { get; }

    internal UnsupportedExpressionException(Expression expression) : base($"Expressions of Type {expression.GetType()} are not supported. {expression}")
    {
        Expression = expression;
        Data[nameof(Expression)] = expression;
    }
}

#endregion
internal class ReplaceParVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _par;
    private readonly Expression _const;

    public ReplaceParVisitor(ParameterExpression par, ConstantExpression @const)
    {
        _par = par;
        _const = @const;
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _par
            ? _const
            : node;

}
internal class ReplaceParVisitor2 : ExpressionVisitor
{
    private readonly ParameterExpression _replace;
    private readonly ParameterExpression _with;
    public ReplaceParVisitor2(ParameterExpression replace, ParameterExpression with)
    {
        _replace = replace;
        _with = with;

    }

    protected override Expression VisitParameter(ParameterExpression node)
        => base.VisitParameter(node == _replace
            ? _with
            : node
        );

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Type != _replace.Type)
            return base.VisitMember(node);
        var updatedMemberInfo = _with.Type.GetMemberWithSameMetadataDefinitionAs(node.Member);
        return base.VisitMember(Expression.MakeMemberAccess(Visit(node.Expression), updatedMemberInfo));
    }
    protected override Expression VisitMemberInit(MemberInitExpression node) =>
        throw new UnsupportedExpressionException(node);
}
