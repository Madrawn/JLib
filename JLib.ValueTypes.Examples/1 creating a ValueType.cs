using JLib.ValueTypes;
using ValidationAttribute = JLib.ValueTypes.ValidationAttribute;
using Xunit;
using FluentAssertions;
using JLib.Exceptions;
using ValueType = JLib.ValueTypes.ValueType;

namespace Examples;

/// <summary>
/// this example shows how to create a value type and how to validate it
/// </summary>
// ReSharper disable once InconsistentNaming
public class Creating_a_ValueType
{
    public record EmailAddress(string Value) : StringValueType(Value)
    {
        /// <summary>
        /// registers this method as validator for this value type.
        /// it will be called when this value type or any derivation of it is created.
        /// </summary>
        [Validation]
        // ReSharper disable once UnusedMember.Local
        private static void Validate(IValidationContext<string?> must)
            => must.Contain("@").Contain(".");
    }

    [Fact]
    public void SuccessfulInstantiation()
    {
        var sut = () => new EmailAddress("my@example.com");
        sut.Should().NotThrow();
    }
    [Fact]
    public void FailingInstantiation()
    {
        var sut = () => new EmailAddress("not a email address");
        sut.Should().Throw<JLibAggregateException>();
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