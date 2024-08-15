using JLib.Exceptions;
using JLib.ValueTypes;
using Xunit;
using ValueType = JLib.ValueTypes.ValueType;

namespace Examples;

// ReSharper disable once InconsistentNaming
public class Custom_ValueType_Definition
{// todo
    public record MyValueType(string Value) : StringValueType(Value);
    [Fact]
    public void test()
    {
        using var ex = new ExceptionBuilder("my ex");
        ex.AddChild(ValueType.GetErrors<MyValueType, string>("myValue"));
        //ex.GetException().GetHierarchyInfoJson()
        //ex.ThrowIfNotEmpty();
    }
}
