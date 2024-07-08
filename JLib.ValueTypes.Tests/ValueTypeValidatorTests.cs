using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Tests;

public record DemoAsciiString(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(StringValidator must)
    => must.BeAscii();
}

public record DemoAlphanumericString(string Value) : DemoAsciiString(Value)
{
    [Validation]
    private static void Validate(StringValidator must)
        => must.BeAlphanumeric();
}

public class ValueTypeValidatorSignatureConstraintTests
{
    private static class InvalidValidatorTypes
    {

        public record PublicValidator(string Value) : DemoAsciiString(Value)
        {
            [Validation]
            public static void Validate(StringValidator v)
                => v.BeAlphanumeric();
        }
        public record ProtectedValidator(string Value) : DemoAsciiString(Value)
        {
            [Validation]
            protected static void Validate(StringValidator v)
                => v.BeAlphanumeric();
        }
        public record NonStaticValidator(string Value) : DemoAsciiString(Value)
        {
            [Validation]
            private void Validate(StringValidator v)
                => v.BeAlphanumeric();
        }
        public record InvalidReturnType(string Value) : DemoAsciiString(Value)
        {
            [Validation]
            private static int Validate(StringValidator v)
            {
                v.BeAlphanumeric();
                return 0;
            }
        }
        public record TooManyParameters(string Value) : DemoAsciiString(Value)
        {
            [Validation]
            private static void Validate(StringValidator v, int invalid)
                => v.BeAlphanumeric();
        }
        public record InvalidParameter(string Value) : DemoAsciiString(Value)
        {
            [Validation]
            private static void Validate(int invalid)
            { }
        }
    }

    [Fact]
    public void PublicValidator()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.PublicValidator))))
            .Should().Throw<Exception>();
    [Fact]
    public void ProtectedValidator()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.ProtectedValidator))))
            .Should().Throw<Exception>();
    [Fact]
    public void NonStaticValidator()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.NonStaticValidator))))
            .Should().Throw<Exception>();
    [Fact]
    public void InvalidReturnType()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.InvalidReturnType))))
            .Should().Throw<Exception>();
    [Fact]
    public void TooManyParameters()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.TooManyParameters))))
            .Should().Throw<Exception>();
    [Fact]
    public void InvalidParameter()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.InvalidParameter))))
            .Should().Throw<Exception>();

}

public class ValueTypeValidatorTests
{
    [Fact]
    public void T1_1()
        => ValueType.GetErrors<DemoAsciiString, string>(null!)
            .GetException().Should().NotBeNull();
    [Fact]
    public void T1_2()
        => ValueType.GetErrors<DemoAsciiString, string>("abc")
            .GetException().Should().BeNull();
    [Fact]
    public void T2_1()
        => ValueType.GetErrors<DemoAsciiString, string>("!")
            .GetException().Should().BeNull();
    [Fact]
    public void T2_2()
        => ValueType.GetErrors<DemoAlphanumericString, string>("!")
            .GetException().Should().NotBeNull();

    [Fact]
    public void T2_3()
        => ValueType.GetErrors<DemoAlphanumericString, string>("\u0868")
            .GetException().Should().NotBeNull();
    [Fact]
    public void T2_4()
        => ValueType.GetErrors<DemoAsciiString, string>("\u0868")
            .GetException().Should().NotBeNull();
}
