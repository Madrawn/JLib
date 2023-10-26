using System.Linq.Expressions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.Data.Authorization;

/// <summary>
/// can be injected by Data Providers to add Authorization
/// </summary>
public interface IAuthorizationInfo<TDataObject> : IAuthorizationInfo
    where TDataObject : class, IDataObject
{
    public Expression<Func<TDataObject, bool>> Expression();
    public bool DataObject(TDataObject dataObject);

    IReadOnlyCollection<TDataObject> AndRaiseException(IReadOnlyCollection<TDataObject> dataObjects)
    {
        dataObjects
            .Select(AndGetException)
            .WhereNotNull()
            .RaiseExceptionIfNotEmpty("some Data Objects are not Authorized");
        return dataObjects;
    }
    TDataObject AndRaiseException(TDataObject dataObject)
    {
        var ex = AndGetException(dataObject);
        if (ex is not null)
            throw ex;
        return dataObject;
    }
    Exception? AndGetException(TDataObject dataObject)
    {
        if (DataObject(dataObject))
            return null;
        throw new UnauthorizedAccessException(
            $"you are not allowed to access the DataObject {typeof(TDataObject).FullClassName()}: {dataObject.Id}");
    }

}
internal class AuthorizationInfo<TDataObject, TS1> : IAuthorizationInfo<TDataObject>
    where TDataObject : class, IDataObject
    where TS1 : notnull
{
    private readonly Func<TS1, Expression<Func<TDataObject, bool>>> _authorizeQueryable;
    private readonly Func<TS1, TDataObject, bool> _authorizeDataObject;
    private readonly IServiceScope _scope;
    public DataObjectType Target { get; }

    public AuthorizationInfo(
        Func<TS1, Expression<Func<TDataObject, bool>>> authorizeQueryable,
        Func<TS1, TDataObject, bool> authorizeDataObject,
        DataObjectType target,
        IServiceScope scope
    )
    {
        _authorizeQueryable = authorizeQueryable;
        _authorizeDataObject = authorizeDataObject;
        _scope = scope;
        Target = target;
    }

    public Expression<Func<TDataObject, bool>> Expression()
        => _authorizeQueryable(_scope.ServiceProvider.GetRequiredService<TS1>());

    public bool DataObject(TDataObject dataObject)
        => _authorizeDataObject(_scope.ServiceProvider.GetRequiredService<TS1>(), dataObject);
}