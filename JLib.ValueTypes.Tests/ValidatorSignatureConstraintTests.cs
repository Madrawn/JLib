using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Tests;

/// <summary>
/// tests whether the validator signature constraints work as expected
/// </summary>
public class ValidatorSignatureConstraintTests
{
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

        public record InterfaceContext(string Value) : StringValueType(Value)
        {
            [Validation]
            private static void Validate(IValidationContext<string> v)
            { }
        }

        public record AbstractContext(string Value) : StringValueType(Value)
        {
            [Validation]
            private static void Validate(InvalidValidator v)
                => v.BeAlphanumeric();

            private abstract class InvalidValidator : StringValidator
            {
                protected InvalidValidator(string? value, Type targetType) : base(value, targetType)
                {
                }
            }
        }
        public record TooManyContextPars(string Value) : StringValueType(Value)
        {
            [Validation]
            private static void Validate(InvalidValidator v)
                => v.BeAlphanumeric();

            private class InvalidValidator : StringValidator
            {
                protected InvalidValidator(string? value, Type targetType, string tooManyPars) : base(value, targetType)
                {
                }
            }
        }
        public record NotEnoughContextPars(string Value) : StringValueType(Value)
        {
            [Validation]
            private static void Validate(InvalidValidator v)
                => v.BeAlphanumeric();

            private class InvalidValidator : StringValidator
            {
                protected InvalidValidator() : base(null!, null!)
                {
                }
            }
        }
        public record FirstContextCtorParInvalidType(string Value) : StringValueType(Value)
        {
            [Validation]
            private static void Validate(InvalidValidator v)
                => v.BeAlphanumeric();

            private abstract class InvalidValidator : StringValidator
            {
                protected InvalidValidator(int? value, Type targetType) : base(value.ToString(), targetType)
                {
                }
            }
        }
        public record SecondContextCtorParInvalidType(string Value) : StringValueType(Value)
        {
            [Validation]
            private static void Validate(InvalidValidator v)
                => v.BeAlphanumeric();

            private abstract class InvalidValidator : StringValidator
            {
                protected InvalidValidator(string? value, int targetType) : base(value, targetType.GetType())
                {
                }
            }
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


    public void InterfaceContext()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.InterfaceContext))))
            .Should().Throw<Exception>(); 
    public void AbstractContext()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.AbstractContext))))
            .Should().Throw<Exception>(); 
    public void TooManyContextPars()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.TooManyContextPars))))
            .Should().Throw<Exception>(); 
    public void NotEnoughContextPars()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.NotEnoughContextPars))))
            .Should().Throw<Exception>(); 
    public void FirstContextCtorParInvalidType()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.FirstContextCtorParInvalidType))))
            .Should().Throw<Exception>(); 
    public void SecondContextCtorParInvalidType()
        => ((Action)(() => ValidationProfile<string>.Get(typeof(InvalidValidatorTypes.SecondContextCtorParInvalidType))))
            .Should().Throw<Exception>();


}