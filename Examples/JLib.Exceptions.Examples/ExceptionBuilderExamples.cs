using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace JLib.Exceptions.Examples;
public class ExceptionBuilderExamples
{
    [Fact]
    public void MinimalCode()
    {
        var exceptions = ExceptionBuilder.Create("Example");

        // does nothing since no exception is added
        exceptions.ThrowIfNotEmpty();
    }

    [Fact]
    public void MinimalCodeWithError()
    {
        var exceptionBuilder = ExceptionBuilder.Create("Example");
        exceptionBuilder.Add(new Exception("Example Exception"));

        Action act = () => exceptionBuilder.ThrowIfNotEmpty();
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void NestedExceptions()
    {
        var exceptionBuilder = ExceptionBuilder.Create("Example");
        exceptionBuilder.Add(new Exception("Example Exception"));
        var child = exceptionBuilder.CreateChild("Children");
        child.Add(new Exception("Exceptions of the child"));

        Action act = () => exceptionBuilder.ThrowIfNotEmpty();
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void GetExceptionWithoutThrowing()
    {
        var exceptionBuilder = ExceptionBuilder.Create("Example");
        exceptionBuilder.Add(new Exception("Example Exception"));

        var exception = exceptionBuilder.GetException();

        exception.Should().BeOfType<JLibAggregateException>();
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
    }
    [Fact]
    public void CustomExceptionProviderFails()
    {
        var exceptionBuilder = ExceptionBuilder.Create("Example");
        var provider = new ExampleExceptionProvider(false);
        exceptionBuilder.AddChild(provider);

        var exception = exceptionBuilder.GetException();

        exception.Should().BeOfType<JLibAggregateException>();
    }
    [Fact]
    public void CustomExceptionProviderSucceeds()
    {
        var exceptionBuilder = ExceptionBuilder.Create("Example");
        var provider = new ExampleExceptionProvider(true);
        exceptionBuilder.AddChild(provider);

        var exception = exceptionBuilder.GetException();

        exception.Should().BeNull();
    }
}
