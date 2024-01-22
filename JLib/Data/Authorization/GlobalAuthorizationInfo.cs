using System.Linq.Expressions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.Data.Authorization;

public interface IUnboundAuthorizationInfo
{
    IAuthorizationInfo Bind(IServiceScope scope);
    DataObjectType Target { get; }
}

public interface IUnboundAuthorizationInfo<TDataObject> : IUnboundAuthorizationInfo
    where TDataObject : class, IDataObject
{
    IAuthorizationInfo IUnboundAuthorizationInfo.Bind(IServiceScope scope)
        => Bind(scope);

    new IAuthorizationInfo<TDataObject> Bind(IServiceScope scope);
}

internal class UnboundAuthorizationInfo<TDataObject, TS1> : IUnboundAuthorizationInfo<TDataObject>
    where TDataObject : class, IDataObject
    where TS1 : notnull
{
    private readonly Func<TS1, Expression<Func<TDataObject, bool>>> _authorizeQueryable;
    private readonly Func<TS1, TDataObject, bool> _authorizeDataObject;

    public IAuthorizationInfo<TDataObject> Bind(IServiceScope scope)
        => new AuthorizationInfo<TDataObject, TS1>(_authorizeQueryable, _authorizeDataObject, Target, scope);

    IAuthorizationInfo IUnboundAuthorizationInfo.Bind(IServiceScope scope)
        => Bind(scope);

    public DataObjectType Target { get; }

    public UnboundAuthorizationInfo(
        Func<TS1, Expression<Func<TDataObject, bool>>> authorizeQueryable,
        Func<TS1, TDataObject, bool> authorizeDataObject,
        ITypeCache typeCache
    )
    {
        _authorizeQueryable = authorizeQueryable;
        _authorizeDataObject = authorizeDataObject;
        Target = typeCache.Get<DataObjectType>(typeof(TDataObject));
    }
}

internal class EmptyUnboundAuthorizationInfo<TDataObject> : IUnboundAuthorizationInfo<TDataObject>
    where TDataObject : class, IDataObject
{
    private readonly ITypeCache _typeCache;

    public EmptyUnboundAuthorizationInfo(ITypeCache typeCache)
    {
        _typeCache = typeCache;
        Target = typeCache.Get<DataObjectType>(typeof(TDataObject));
    }

    public IAuthorizationInfo<TDataObject> Bind(IServiceScope scope)
        => new AuthorizationInfo<TDataObject, IServiceScope>(
            _ => _ => true,
            (_, _) => true,
            _typeCache.Get<DataObjectType>(typeof(TDataObject)),
            scope);

    public DataObjectType Target { get; }
}