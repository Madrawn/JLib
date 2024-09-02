using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using JLib.Helper;

namespace JLib.Exceptions.Examples;
public class ExceptionBuilderExamples
{
    [Fact]
    public void MinimalCode()
    {
        var exceptions = new ExceptionBuilder("Example");

        // does nothing since no exception is added
        exceptions.ThrowIfNotEmpty();
    }

    [Fact]
    public void MinimalCodeWithError()
    {
        var exceptionBuilder = new ExceptionBuilder("Example");
        exceptionBuilder.Add(new Exception("Example Exception"));

        Action act = () => exceptionBuilder.ThrowIfNotEmpty();
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void NestedExceptions()
    {
        var exceptionBuilder = new ExceptionBuilder("Example");
        exceptionBuilder.Add(new Exception("Example Exception"));
        var child = exceptionBuilder.CreateChild("Children");
        child.Add(new Exception("Exceptions of the child"));

        Action act = () => exceptionBuilder.ThrowIfNotEmpty();
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void GetExceptionWithoutThrowing()
    {
        var exceptionBuilder = new ExceptionBuilder("Example");
        exceptionBuilder.Add(new Exception("Example Exception"));

        var exception = exceptionBuilder.GetException();

        exception.Should().BeOfType<JLibAggregateException>();
    }

    [Fact]
    public void UsingDisposables()
    {
        var act = () =>
        {
            using var exceptionBuilder = new ExceptionBuilder("Example");
            exceptionBuilder.Add(new Exception("ExampleException"));
        };
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void UsingDisposablesWithNestedExceptions()
    {
        var act = () =>
        {
            using var exceptionBuilder = new ExceptionBuilder("Example");
            exceptionBuilder.Add(new Exception("ExampleException"));
            using var childBuilder = exceptionBuilder.CreateChild("child");
            exceptionBuilder.Add(new Exception("ExampleChildException"));
        };
        act.Should().Throw<JLibAggregateException>();
    }

    public class ExampleExceptionProvider : IExceptionProvider
    {
        private readonly bool _isValid;

        public ExampleExceptionProvider(bool isValid)
            => _isValid = isValid;

        public Exception? GetException()
            => _isValid
                ? null
                : new Exception("Data is Invalid");

        public bool HasErrors() => !_isValid;
    }
    [Fact]
    public void CustomExceptionProviderFails()
    {
        var exceptionBuilder = new ExceptionBuilder("Example");
        var provider = new ExampleExceptionProvider(false);
        exceptionBuilder.AddChild(provider);

        var exception = exceptionBuilder.GetException();

        exception.Should().BeOfType<JLibAggregateException>();
    }
    [Fact]
    public void CustomExceptionProviderSucceeds()
    {
        var exceptionBuilder = new ExceptionBuilder("Example");
        var provider = new ExampleExceptionProvider(true);
        exceptionBuilder.AddChild(provider);

        var exception = exceptionBuilder.GetException();

        exception.Should().BeNull();
    }
}


public class MyCustomException : Exception
{
    public int InvalidValue { get; }

    public MyCustomException(int invalidValue) : base($"value {invalidValue} is invalid")
    {
        InvalidValue = invalidValue;
        Data[nameof(InvalidValue)] = invalidValue;
    }
}