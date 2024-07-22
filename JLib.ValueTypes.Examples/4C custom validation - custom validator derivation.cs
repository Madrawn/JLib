using FluentAssertions;
using JLib.ValueTypes;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Examples;

/// <summary>
/// this is only recommended if you want to override the exception generation.
/// </summary>
// ReSharper disable once InconsistentNaming
public class Custom_Validation_using_custom_validator_derivation
{// todo
    public class EmailValidator : ValidationContext<string?>
    {
        public EmailValidator(string? value, Type targetType) : base(value, targetType)
        {
        }

        public EmailValidator BeAnEmail()
        {
            this.Contain('@');
            this.Contain('.');
            this.NotEndWith('.');
            return this;
        }
    }

    public record EmailAddress(string Value) : StringValueType(Value)
    {
        /// <summary>
        /// registers this method as validator for this value type.
        /// it will be called when this value type or any derivation of it is created.
        /// </summary>
        [Validation]
        private static void Validate(EmailValidator must)
            => must.BeAnEmail();
    }

    [Fact]
    public void Valid()
    {
        var sut = () => new EmailAddress("my@example.de");
        sut.Should().NotThrow();
    }
    [Fact]
    public void Invalid()
    {
        var sut = () => new EmailAddress("example.de");
        sut.Should().Throw<AggregateException>();
    }
}