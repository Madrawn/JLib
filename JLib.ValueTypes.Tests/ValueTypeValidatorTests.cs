using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JLib.Exceptions;
using JLib.Helper;
using Xunit;

namespace JLib.ValueTypes.Tests;

public record DemoAsciiString(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(StringValidator v)
        => v.BeAscii();
}
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
public record DemoAlphanumericString(string Value) : DemoAsciiString(Value)
{
    [Validation]
    private static void Validate(StringValidator v)
        => v.BeAlphanumeric();
}

public class ValueTypeValidatorTests
{
    [Fact]
    public void T1_1()
        => ValueType.GetErrors<DemoAsciiString, string>(null!)
            .GetException().Should().BeNull();
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
            .GetException().Should().BeNull();
}
