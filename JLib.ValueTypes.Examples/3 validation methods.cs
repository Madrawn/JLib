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
        private static void Validate(ValidationContext<string?> must)
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