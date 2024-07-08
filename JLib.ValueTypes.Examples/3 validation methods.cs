using FluentAssertions;
using JLib.Exceptions;
using JLib.ValueTypes;
using Xunit;
using ValueType = JLib.ValueTypes.ValueType;

namespace Examples;

public class ValidationMethods
{
    public record EmailAddress(string Value) : StringValueType(Value)
    {
        [Validation]
        private static void Validate(StringValidator must)
            => must.Contain("@").Contain(".");
    }

    [Fact]
    public void ValidateViaConstructor()
    {
        var sut = () => new EmailAddress("example.de");
        sut.Should().Throw<AggregateException>();
    }

    [Fact]
    public void StaticValidationValid()
    {
        ValueType.GetErrors<EmailAddress, string>("not a mail")
            .HasErrors()
            .Should().BeTrue();
    }

    [Fact]
    public void StaticValidationInvalid()
    {
        ValueType.GetErrors<EmailAddress, string>("my@example.de")
            .HasErrors()
            .Should().BeFalse();
    }
    [Fact]
    public void ExceptionDebugInfo()
    {
        var res = ValueType.GetErrors<EmailAddress, string>("example");
        var ex = res.GetException();
        // note: the content is not optimized to be human readable by replacing single item arrays with just the item.
        // for parsing, it is recommended to interpret the exception itself.
        // do not use this json for production code
        ex?.GetHierarchyInfoJson()
            .Should().Be(@"{
  ""Message"": ""Examples.ValidationMethods.EmailAddress"",
  ""JLibAggregateException"": {
    ""Message"": ""Examples.ValidationMethods.EmailAddress validation failed: \u0027example\u0027 is not a valid Value."",
    ""JLibAggregateException"": {
      ""Message"": ""Value Validation Failed"",
      ""2 ValidationException"": [
        ""Value must contain @"",
        ""Value must contain .""
      ]
    }
  }
}");
    }

    [Fact]
    public void TryCreateSucceeds()
    {
        var value = ValueType.TryCreate<EmailAddress, string>(
            "my@example.com", out var errors);
        value.Should().NotBeNull();
        errors.HasErrors().Should().BeFalse();
    }

    [Fact]
    public void TryCreateFails()
    {
        var value = ValueType.TryCreate<EmailAddress, string>(
            "not a email address", out var errors);
        value.Should().BeNull();
        errors.HasErrors().Should().BeTrue();
    }
}