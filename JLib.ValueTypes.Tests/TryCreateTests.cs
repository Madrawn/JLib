﻿using FluentAssertions;
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

    public record PositiveInt(int Value) : IntValueType(Value)
    {
        [Validation]
        private static void Validate(ValidationContext<int> must)
            => must.BePositive();
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