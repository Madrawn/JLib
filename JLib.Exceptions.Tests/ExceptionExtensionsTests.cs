using System.Text.Json;
using JLib.Exceptions;
using Snapshooter.Xunit;
using Xunit;

namespace JLib.ExceptionsTests;

public class ExceptionToJsonTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    [Fact]
    public void OnException()
    {
        new Exception("Test")
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }
    [Fact]
    public void OnDerivedException()
    {
        new IndexOutOfRangeException("Test")
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }
    [Fact]
    public void WithInnerException()
    {
        new Exception("Test", new("inner"))
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }

    [Fact]
    public void EmptyAggregateException()
    {
        new AggregateException("Test")
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }

    [Fact]
    public void AggregateException1()
    {
        new AggregateException("Test", new Exception("inner"))
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }
    [Fact]
    public void JLibAggregateException1()
    {
        new JLibAggregateException("Test", new[] { new Exception("inner") })
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }
    [Fact]
    public void JLibAggregateException2()
    {
        new JLibAggregateException("Test", new[]
            {
                new Exception("inner 1"),
                new Exception("inner 2"),
                new IndexOutOfRangeException("inner oor 1"),
                new IndexOutOfRangeException("inner oor 2"),
                new IndexOutOfRangeException("inner oor 3",new ("sub nested oor")),
                new JLibAggregateException("sub ex",new []
                {
                    new Exception("inner",
                        new IndexOutOfRangeException("sub inner"))
                }),
                new AggregateException("sub ex 2",new[]
                {
                    new Exception("inner 2")
                })
            })
            .GetHierarchyInfoJson(Options)
            .MatchSnapshot();
    }
}