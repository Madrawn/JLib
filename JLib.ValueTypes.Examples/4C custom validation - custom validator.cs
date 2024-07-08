using System.ComponentModel.DataAnnotations;
using JLib.ValueTypes;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Examples;
// ReSharper disable once InconsistentNaming
public static class Custom_Validation_using_a_custom_validator
{
    /// <summary>
    /// this method is recommended if you reuse the same validation logic in multiple places and only use the built-in validator
    /// </summary>
    public static StringValidator ContainMyExample1(this StringValidator validator)
    {
        var testString = "MyExample1";
        if (!validator.Value?.Contains(testString) == true)
            throw new AggregateException($"'{validator.Value}' does not contain '{testString}'");
        return validator;
    }

    public static ValueValidator<string> ContainMyExample2(this ValueValidator<string> validator)
    {
        var testString = "MyExample2";
        if (validator.Value.Contains("MyExample2"))
            if (!validator.Value.EndsWith(testString))
                throw new AggregateException($"'{validator.Value}' does not end with '{testString}'");
        return validator;
    }

    public static IValueValidator<string> ContainMyExample3(this IValueValidator<string> validator)
    {
        var testString = "MyExample3";
        if (validator.Value.Contains(testString))
            throw new AggregateException($"'{validator.Value}' does not end with '{testString}'");
        return validator;
    }
}

public record EmailAddress(string Value) : StringValueType(Value)
{
    /// <summary>
    /// registers this method as validator for this value type.
    /// it will be called when this value type or any derivation of it is created.
    /// </summary>
    [JLib.ValueTypes.Validation]
    private static void Validate(StringValidator must)
        => must.Contain("@").Contain(".");
}
