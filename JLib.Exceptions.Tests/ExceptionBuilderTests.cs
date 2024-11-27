using System.Reflection;
using System.Text.Json;

using FluentAssertions;

using JLib.Helper;

using Snapshooter.Xunit;

using Xunit;

namespace JLib.Exceptions.Tests;

public class ExceptionBuilderTests
{
    private readonly ExceptionBuilder _builder = new("test");
    [Fact]
    public void HasErrors_NoErrors()
        => _builder.HasErrors().Should().BeFalse();

    [Fact]
    public void HasErrors_NoErrorsWithChild()
    {
        _builder.CreateChild("child");
        _builder.HasErrors().Should().BeFalse();
    }

    [Fact]
    public void HasErrors_ErrorOnChild()
    {
        var child = _builder.CreateChild("child");
        child.Add(new Exception("error"));
        _builder.HasErrors().Should().BeTrue();
    }
    [Fact]
    public void HasErrors_ErrorOnRootWithChild()
    {
        var child = _builder.CreateChild("child");
        _builder.Add(new Exception("error"));
        _builder.HasErrors().Should().BeTrue();
    }

    [Fact]
    public void Using_NoErrors()
    {
        using var _ = _builder;
    }

    [Fact]
    public void Using_WithChild()
    {
        _builder.CreateChild("child");
        using var _ = _builder;
    }
    [Fact]
    public void Using_WithContent()
    {
        _builder.Add(new Exception("ex"));
        Action act = () =>
        {
            using var _ = _builder;
        };
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void Using_ContentOnChild()
    {
        var child = _builder.CreateChild("sub");
        child.Add(new Exception("content"));
        Action act = () =>
        {
            using var _ = _builder;
        };
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void Build_Content()
    {
        _builder.Add(new Exception("content"));
        JsonSerializer.Serialize(_builder.GetException(), new JsonSerializerOptions() { WriteIndented = true })
            .MatchSnapshot();

        //            .Should().Be(@"{
        //  ""TargetSite"": null,
        //  ""Message"": ""test\r\n\u251C\u2500 Inner Exceptions\r\n\u2502  \u251C\u2500 1 Exception\r\n\u2502  \u2502  \u251C\u2500 content\r\n\u2502  \u2502  \u2502  \r\n\r\n"",
        //  ""Data"": {},
        //  ""InnerException"": {
        //    ""TargetSite"": null,
        //    ""Message"": ""content"",
        //    ""Data"": {},
        //    ""InnerException"": null,
        //    ""HelpLink"": null,
        //    ""Source"": null,
        //    ""HResult"": -2146233088,
        //    ""StackTrace"": null
        //  },
        //  ""HelpLink"": null,
        //  ""Source"": null,
        //  ""HResult"": -2146233088,
        //  ""StackTrace"": null
        //}");
    }

    [Fact]
    [Trait("Bug", "53031")]
    public void DisposeWorksAsExpected_WithError()
    {
        var child = _builder.CreateChild("child");
        child.Add("exception");
        child.Dispose();
        var act = () => _builder.Dispose();
        act.Should().Throw<AggregateException>();
    }
    [Fact]
    public void DisposeWorksAsExpected_WithoutError()
    {
        var childrenField = (typeof(ExceptionBuilder).GetField("_children", BindingFlags.Instance | BindingFlags.NonPublic) ??
                             throw new("get children field failed"));

        GetChildren().Should().BeEmpty();

        var child = _builder.CreateChild("child");
        GetChildren().Should().HaveCount(1);
        child.Dispose();
        GetChildren().Should().BeEmpty();

        
        _builder.Dispose();
        
        return;

        IReadOnlyCollection<IExceptionProvider> GetChildren() =>
            ((List<IExceptionProvider>)childrenField.GetValue(_builder)!).ToReadOnlyCollection();
    }



}