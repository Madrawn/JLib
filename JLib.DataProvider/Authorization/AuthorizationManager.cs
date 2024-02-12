using System.Collections.Concurrent;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataProvider.Authorization;

public interface IAuthorizationManager
{
    public IAuthorizationInfo<TDo> Get<TDo>(IServiceScope requestScope)
        where TDo : class, IDataObject;

    public IAuthorizationInfo Get(DataObjectType type, IServiceScope requestScope);
}

public class AuthorizationManager : IAuthorizationManager
{
    private readonly ITypeCache _typeCache;
    private readonly ConcurrentDictionary<Type, IUnboundAuthorizationInfo> _profiles;

    public IAuthorizationInfo<TDo> Get<TDo>(IServiceScope requestScope)
        where TDo : class, IDataObject
        => Get(_typeCache.Get<DataObjectType>(typeof(TDo)), requestScope)
            .CastTo<IAuthorizationInfo<TDo>>();

    public IAuthorizationInfo Get(DataObjectType type, IServiceScope requestScope)
    {
        return _profiles
            .GetValueOrAdd(
                type.Value,
                () => Activator.CreateInstance(
                          typeof(EmptyUnboundAuthorizationInfo<>)
                              .MakeGenericType(type.Value), _typeCache
                      )?.CastTo<IUnboundAuthorizationInfo>()
                      ?? throw new InvalidSetupException("default authInfo could not be created")
            ).Bind(requestScope);
    }

    public AuthorizationManager(ITypeCache typeCache)
    {
        _typeCache = typeCache;
        _profiles = typeCache.All<AuthorizationProfileType>()
            .SelectMany(x => x.Instance.Build())
            .ToConcurrentDictionary(x => x.Target.Value);
    }
}