using JLib.Helper;
using JLib.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static JLib.DataGeneration.DataPackageValues;

namespace JLib.DataGeneration;

public interface IDataPackageRetriever
{
    TPackage GetPackage<TPackage>()
        where TPackage : DataPackage;
}
public interface IDataPackageStore : IDataPackageRetriever
{
    IServiceProvider ServiceProvider { get; }
    /// <summary>
    /// sets the id of the given <paramref name="property"/> to the persisted id or creates a new one<br/>
    /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal void SetIdPropertyValue(PropertyInfo property);

    /// <summary>
    /// returns a named id which is not 
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    TId GetNamedId<TId>(IdName name);

    internal void Bind(DataPackage dataPackage);
}

public sealed partial class DataPackageManager
{
    private class DataPackageStore : IDataPackageStore
    {
        private readonly DataPackageManager _manager;
        private readonly Type _dataPackage;

        internal DataPackageStore(DataPackageManager manager, Type dataPackage)
        {
            _manager = manager;
            _dataPackage = dataPackage;
        }

        IServiceProvider IDataPackageStore.ServiceProvider
            => _manager._dataServiceProvider;


        /// <summary>
        /// sets the id of the given <paramref name="property"/> to the persisted id or creates a new one<br/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void IDataPackageStore.SetIdPropertyValue(PropertyInfo property)
        {
            var packageType = property.ReflectedType
                ?? throw new Exception("Property has no Reflected type");

            var packageInstance = _manager._packageProvider.GetRequiredService(packageType);

            var id = GetId(new(property), property.PropertyType);
            property.SetValue(packageInstance, id);
        }

        /// <summary>
        /// gets the id with the given <paramref name="identifier"/> of the given <paramref name="idType"/><br/>
        /// throws a <see cref="ArgumentOutOfRangeException"/> when the property is neither a <see cref="int"/>, <see cref="Guid"/>, <see cref="IntValueType"/> nor <see cref="GuidValueType"/>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private object GetId(IdIdentifier identifier, Type idType)
        {
            if (idType == typeof(int))
                return _manager._idRegistry.GetIntId(identifier);
            if (idType.IsAssignableTo(typeof(IntValueType)))
            {
                var nativeId = _manager._idRegistry.GetIntId(identifier);
                return _manager._mapper.Map(nativeId, idType);
            }
            if (idType == typeof(Guid))
                return _manager._idRegistry.GetGuidId(identifier);
            if (idType.IsAssignableTo(typeof(GuidValueType)))
            {
                var nativeId = _manager._idRegistry.GetGuidId(identifier);
                return _manager._mapper.Map(nativeId, idType);
            }

            throw new ArgumentOutOfRangeException(nameof(idType), "unknown type");
        }

        /// <summary>
        /// returns a named id which is not 
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        TId IDataPackageStore.GetNamedId<TId>(IdName name)
            => GetId(new(new(_dataPackage), new("named_" + name.Value)), typeof(TId)).CastTo<TId>();

        public TPackage GetPackage<TPackage>() where TPackage : DataGeneration.DataPackage
            => _manager.GetPackage<TPackage>();
    }
}
