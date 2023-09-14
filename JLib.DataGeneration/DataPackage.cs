using JLib.Data;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.DataGeneration;

[IsDerivedFromAny<DataPackage<IEntity>>, NotAbstract]
public record DataPackageType(Type Value) : TypeValueType(Value);

public abstract class DataPackage<TEntity> where TEntity : IEntity
{
    public DataPackage()
    {
        //typeof(DataPackage<>).
    }
}
