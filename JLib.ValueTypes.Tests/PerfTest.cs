using FluentAssertions;

using JLib.Exceptions;

using Microsoft.VisualStudio.TestPlatform.Utilities;

using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

namespace JLib.ValueTypes.Tests;

public class PerfTest
{
    private static Random random = new Random();
    private readonly ITestOutputHelper output;

    public PerfTest(ITestOutputHelper output)
    {
        this.output = output;
    }

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

    public static FiveCharacterString FiveCharacterStringCreate(Func<string> rand) => ValueType.Create<FiveCharacterString, string>(rand());

    public static string GetRandom5LetterString()
    {
        var str = random.Next(10000, 99999).ToString();
        return $"{str}";
    }

    public static Func<int> GetRandomPositiveInt(Random random) => () => random.Next(1, short.MaxValue);

    public static List<T> PerformanceTest<T>(int iterations, Func<object> action, ITestOutputHelper output)
    {
        var values = new List<T>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        for (var i = 0; i < iterations; i++)
        {
            values.Add((T)action());
        }

        stopwatch.Stop();
        output.WriteLine($"Time taken to create {iterations} instances of {typeof(T).Name}: {stopwatch.ElapsedMilliseconds} ms");
        return values;
    }

    [Fact]
    public void Performance()
    {
        FiveCharacterStringCreate(GetRandom5LetterString);
        FiveCharacterStringCreate(GetRandom5LetterString);
        // Performance Tests

        // Get initial memory usage
        long initialMemory, finalMemory;
        initialMemory = GC.GetTotalMemory(true);

        var l1 = PerformanceTest<FiveCharacterString>(short.MaxValue * 100, () => FiveCharacterStringCreate(GetRandom5LetterString), output);
        // Get final memory usage
        finalMemory = GC.GetTotalMemory(true);
        output.WriteLine($"Memory used to create instances: {(finalMemory - initialMemory) / 1024 / 1024} kilobytes");
    }

    [Fact]
    public void ProofOfConcept()
    {
        const int iterations = short.MaxValue * 1000;
        var stopwatch = new Stopwatch();

        // Measure time for "a" + "b" + "c"
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            var str = GetRandom5LetterString() + GetRandom5LetterString() + GetRandom5LetterString();
        }
        stopwatch.Stop();
        var concatenationTime = stopwatch.ElapsedMilliseconds;
        output.WriteLine($"Time taken for concatenation using + operator: {concatenationTime} ms");

        // Measure time for $"{"a"}{"b"}{"c"}"
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var str = $"{GetRandom5LetterString()}{GetRandom5LetterString()}{GetRandom5LetterString()}";
        }
        stopwatch.Stop();
        var interpolationTime = stopwatch.ElapsedMilliseconds;
        output.WriteLine($"Time taken for concatenation using string interpolation: {interpolationTime} ms");

        // Output the results
        output.WriteLine($"interpolationTime - concatenationTime < 0:{interpolationTime - concatenationTime < 0}");
        output.WriteLine($"Difference in time: {interpolationTime - concatenationTime} ms");
    }

    [Fact]
    public void StringIsValid()
    {
        var value = ValueType.TryCreate<FiveCharacterString, string>("valid", out var error);
        error.HasErrors().Should().BeFalse();
        value?.Value.Should().Be("valid");
    }
}