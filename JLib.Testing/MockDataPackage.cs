using static JLib.FactoryAttributes.TvtFactoryAttributes;


namespace JLib.Testing;

public static class ValueTypes
{
    [IsDerivedFrom(typeof(Testing.MockDataPackage)), NotAbstract]
    public record MockDataPackage(Type Value) : TypeValueType(Value);
}


public abstract class MockDataPackage
{
    public MockDataPackage()
    {
        
    }
}

public class MockDataPackageManager
{

}
