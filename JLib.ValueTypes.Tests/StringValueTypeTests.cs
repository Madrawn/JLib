using FluentAssertions;
using JLib.Exceptions;
using JLib.Helper;
using Xunit;

namespace JLib.ValueTypes.Tests;
public class StringValueTypeTests
{
    public record TestArgument(string? TestString, string ValidatorName, Action<StringValidator> Validator,
        bool ErrorExpected);

    public static IEnumerable<object[]> Arguments => new TestArgument[]
    {
        new("",nameof(StringValidator.NotBeNull),v=>v.NotBeNull(),false),
        new(null,nameof(StringValidator.NotBeNull),v=>v.NotBeNull(),true),
        new("a",nameof(StringValidator.NotBeNull),v=>v.NotBeNull(),false),
        new(" ",nameof(StringValidator.NotBeNull),v=>v.NotBeNull(),false),
        new("a ",nameof(StringValidator.NotBeNull),v=>v.NotBeNull(),false),

        new("",nameof(StringValidator.NotBeNullOrEmpty),v=>v.NotBeNullOrEmpty(),true),
        new(null,nameof(StringValidator.NotBeNullOrEmpty),v=>v.NotBeNullOrEmpty(),true),
        new("a",nameof(StringValidator.NotBeNullOrEmpty),v=>v.NotBeNullOrEmpty(),false),
        new(" ",nameof(StringValidator.NotBeNullOrEmpty),v=>v.NotBeNullOrEmpty(),false),
        new("a ",nameof(StringValidator.NotBeNullOrEmpty),v=>v.NotBeNullOrEmpty(),false),

        new("",nameof(StringValidator.NotBeNullOrWhitespace),v=>v.NotBeNullOrWhitespace(),true),
        new(null,nameof(StringValidator.NotBeNullOrWhitespace),v=>v.NotBeNullOrWhitespace(),true),
        new("a",nameof(StringValidator.NotBeNullOrWhitespace),v=>v.NotBeNullOrWhitespace(),false),
        new(" ",nameof(StringValidator.NotBeNullOrWhitespace),v=>v.NotBeNullOrWhitespace(),false),
        new("a ",nameof(StringValidator.NotBeNullOrWhitespace),v=>v.NotBeNullOrWhitespace(),false),

        new("",nameof(StringValidator.NotContainWhitespace),v=>v.NotContainWhitespace(),false),
        new(null,nameof(StringValidator.NotContainWhitespace),v=>v.NotContainWhitespace(),false),
        new("a",nameof(StringValidator.NotContainWhitespace),v=>v.NotContainWhitespace(),false),
        new(" ",nameof(StringValidator.NotContainWhitespace),v=>v.NotContainWhitespace(),true),
        new("a ",nameof(StringValidator.NotContainWhitespace),v=>v.NotContainWhitespace(),true),

        new("",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),false),
        new(null,nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),false),
        new("a",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),false),
        new(" ",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),false),
        new("a ",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),false),
        new("<a ",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),true),
        new("a< ",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),true),
        new("a <",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),true),
        new("<a <",nameof(StringValidator.NotContain)+" <",v=>v.NotContain("<"),true),

        new("",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),true),
        new(null,nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),true),
        new("a",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),true),
        new(" ",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),true),
        new("a ",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),true),
        new("prefa ",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),false),
        new("pref a ",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),false),
        new("pref a pref",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),false),
        new("a pref",nameof(StringValidator.StartWith)+" pref",v=>v.StartWith("pref"),true),

        new("",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new(null,nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new("a",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new(" ",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new("a ",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new("https://www.google.de",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),false),
        new("https://www.google.de?query=od",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),false),
        new("https://www.google.de?query=od&other=that",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),false),
        new("https://www.google.de<",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new("https://www.google.de?query=od<",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),
        new("https://www.google.de?query=od&other=that<",nameof(StringValidator.BeHttpsUrl),v=>v.BeHttpsUrl(),true),

        new("",nameof(StringValidator.MinimumLength),v=>v.MinimumLength(1),true),
        new(null,nameof(StringValidator.MinimumLength),v=>v.MinimumLength(1),true),
        new("a",nameof(StringValidator.MinimumLength),v=>v.MinimumLength(1),false),
        new("ab",nameof(StringValidator.MinimumLength),v=>v.MinimumLength(1),false),

        new("",nameof(StringValidator.MaximumLength),v=>v.MaximumLength(1),false),
        new(null,nameof(StringValidator.MaximumLength),v=>v.MaximumLength(1),true),
        new("a",nameof(StringValidator.MaximumLength),v=>v.MaximumLength(1),false),
        new("ab",nameof(StringValidator.MaximumLength),v=>v.MaximumLength(1),true),


        new("",nameof(StringValidator.BeOfLength),v=>v.BeOfLength(1),true),
        new(null,nameof(StringValidator.BeOfLength),v=>v.BeOfLength(1),true),
        new("a",nameof(StringValidator.BeOfLength),v=>v.BeOfLength(1),false),
        new("ab",nameof(StringValidator.BeOfLength),v=>v.BeOfLength(1),true),

        new("",nameof(StringValidator.BeNumeric),v=>v.BeNumeric(),false),
        new(null,nameof(StringValidator.BeNumeric),v=>v.BeNumeric(),true),
        new("a",nameof(StringValidator.BeNumeric),v=>v.BeNumeric(),true),
        new("1",nameof(StringValidator.BeNumeric),v=>v.BeNumeric(),false),
        new("#",nameof(StringValidator.BeNumeric),v=>v.BeNumeric(),true),

        new("",nameof(StringValidator.BeAlphanumeric),v=>v.BeAlphanumeric(),false),
        new(null,nameof(StringValidator.BeAlphanumeric),v=>v.BeAlphanumeric(),true),
        new("a",nameof(StringValidator.BeAlphanumeric),v=>v.BeAlphanumeric(),false),
        new("1",nameof(StringValidator.BeAlphanumeric),v=>v.BeAlphanumeric(),false),
        new("#",nameof(StringValidator.BeAlphanumeric),v=>v.BeAlphanumeric(),true),

        new("",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
        new(null,nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
        new("a",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),false),
        new("b",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),false),
        new("1",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
        new("#",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
        new("A",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
        new("ab",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
        new("ba",nameof(StringValidator.BeAlphanumeric),v=>v.BeOneOf(new[]{"a","b"}),true),
    }.Select(x => new object[] { x });

    [Theory]
    [MemberData(nameof(Arguments))]
    public void Test(TestArgument argument)
    {
        var val = new StringValidator(argument.TestString!, "Test");
        argument.Validator(val);
        var exProv = val.CastTo<IExceptionProvider>();
        var ex = exProv.GetException();


        if (argument.ErrorExpected)
            ex.Should().NotBeNull();
        else
            ex.Should().BeNull();

    }


    public record TestStringValueType(string Value) : StringValueType(Value);

}
