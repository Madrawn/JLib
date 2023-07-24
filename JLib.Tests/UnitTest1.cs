namespace JLib.Tests;
public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var typeCache = new TypeCache(typeof(UnitTest1).Assembly, typeof(TypeValueType).Assembly);
    }
}
