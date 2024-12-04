using FluentAssertions;

using JLib.Exceptions;

using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Diagnostics;

using Xunit;
using Xunit.Abstractions;

namespace JLib.ValueTypes.Tests;
public class PerfTest
{
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

    [Fact]
    public void StringIsValid()
    {
        var value = ValueType.TryCreate<FiveCharacterString, string>("valid", out var error);
        error.HasErrors().Should().BeFalse();
        value?.Value.Should().Be("valid");
    }

    [Fact]
    public void Performance()
    {
        FiveCharacterStringCreate(GetRandom5LetterString);
        FiveCharacterStringCreate(GetRandom5LetterString);
        // Performance Tests

        ///*Time taken to create 3276700 instances of PositiveInt: 661 ms
        //Memory used to create instances: 107 kilobytes*/
        //var getInt = GetRandomPositiveInt(new Random());
        // Get initial memory usage
        long initialMemory, finalMemory;
        initialMemory = GC.GetTotalMemory(true);

        var l1 = PerformanceTest<FiveCharacterString>(short.MaxValue * 100, () => FiveCharacterStringCreate(GetRandom5LetterString), output);
        // Get final memory usage
        finalMemory = GC.GetTotalMemory(true);
        output.WriteLine($"Memory used to create instances: {(finalMemory - initialMemory) / 1024 / 1024} kilobytes");

    }




    //public static PositiveInt PositiveIntCreate(Func<int> getInt) => new(getInt());
    //public static PositiveInt GenericPositiveIntCreate(Func<int> getInt) => (PositiveInt)TestValueTypeBase.GetValueTypeInstance(typeof(PositiveInt), getInt());

    public static FiveCharacterString FiveCharacterStringCreate(Func<string> rand) => ValueType.Create<FiveCharacterString, string>(rand());
    //public static Email EmailCreate() => new(GetRandomEmail());
    //public static Email GenericEmailCreate() => (Email)TestValueTypeBase.GetValueTypeInstance(typeof(Email), GetRandomEmail());


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

    public static string GetRandom5LetterString()
    {
        var str = Guid.NewGuid().ToString()[..5];
        return $"{str}";
    }

    public static Func<int> GetRandomPositiveInt(Random random) => () => random.Next(1, short.MaxValue);
}
