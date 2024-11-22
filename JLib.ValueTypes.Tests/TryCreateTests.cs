using FluentAssertions;
using JLib.Exceptions;
using Xunit;

namespace JLib.ValueTypes.Tests;

/// <summary>
/// tests whether <see cref="ValueType.TryCreate{TVt,T}(T,out IExceptionProvider)"/> works as expected
/// </summary>
public class TryCreateTests
{

    public record FiveCharacterString(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(5);
    }
    public record ThreeCharacterString(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<string?> must)
            => must.BeOfLength(3);
    }

    public record PositiveInt(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> must)
            => must.BePositive();
    }
    public record NegativeInt(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> must)
            => must.BeNegative();
    }


    [Fact]
    public void ClassFullyGeneric()
    {
        var t1 = ValueType.TryCreate<FiveCharacterString, string>("12345", out var errors1);
        var t2 = ValueType.TryCreate<ThreeCharacterString, string>("123", out var errors2);
        errors1.GetException()?.ToString().Should().BeNull();
        errors2.GetException()?.ToString().Should().BeNull();
        t1.Should().BeOfType<FiveCharacterString>();
        t2.Should().BeOfType<ThreeCharacterString>();
    }










    [Fact]
    public void StringIsValid()
    {
        var value = ValueType.TryCreate<FiveCharacterString, string>("valid", out var error);
        error.HasErrors().Should().BeFalse();
        value?.Value.Should().Be("valid");
    }
    [Fact]
    public void StringValueIsInvalid()
    {
        var value = ValueType.TryCreate<FiveCharacterString, string>("invalid!", out var error);
        error.HasErrors().Should().BeTrue();
        value.Should().BeNull();
    }
    [Fact]
    public void NullableStructValueIsNull()
    {
        var value = ValueType.TryCreate<PositiveInt, int>(null, out var error);
        error.HasErrors().Should().BeFalse();
        value.Should().BeNull();
    }
    [Fact]
    public void NullableStructValueIsValid()
    {
        var value = ValueType.TryCreate<PositiveInt, int>((int?)5, out var error);
        error.HasErrors().Should().BeFalse();
        value?.Value.Should().Be(5);
    }
    [Fact]
    public void NullableStructValueIsInvalid()
    {
        var value = ValueType.TryCreate<PositiveInt, int>((int?)-5, out var error);
        error.HasErrors().Should().BeTrue();
        value.Should().BeNull();
    }
    [Fact]
    public void StructValueIsValid()
    {
        var value = ValueType.TryCreate<PositiveInt, int>(5, out var error);
        error.HasErrors().Should().BeFalse();
        value?.Value.Should().Be(5);
    }
    [Fact]
    public void StructValueIsInvalid()
    {
        var value = ValueType.TryCreate<PositiveInt, int>(-5, out var error);
        error.HasErrors().Should().BeTrue();
        value.Should().BeNull();
    }
    [Fact]
    public void StringValueIsNull()
    {
        var value = ValueType.TryCreate<FiveCharacterString, string>(null, out var error);
        error.HasErrors().Should().BeFalse();
        value.Should().BeNull();
    }
}