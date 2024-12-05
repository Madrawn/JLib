using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;


using JLib.Exceptions;


using System.Diagnostics;


namespace JLib.ValueTypes.Tests;

public class PerfTest
{
    private static Random random = new Random();

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

    public static List<T> PerformanceTest<T>(int iterations, Func<object> action)
    {
        var values = new List<T>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        for (var i = 0; i < iterations; i++)
        {
            values.Add((T)action());
        }

        stopwatch.Stop();
        return values;
    }

    [InProcess]
    public class StringPerformanceBenchmarks
    {
        private static readonly List<string> _strings = Enumerable.Range(0, 5000).Select(_ => GetRandom5LetterString()).ToList();
        private static readonly Func<string> _getRandom5LetterString = GetNextString;

        private static IEnumerator<string> _stringEnumerator = _strings.GetEnumerator();

        private static string GetNextString()
        {
            if (!_stringEnumerator.MoveNext())
            {
                _stringEnumerator = _strings.GetEnumerator();
                _stringEnumerator.MoveNext();
            }
            return _stringEnumerator.Current;
        }
        [GlobalSetup]
        public void GlobalSetup()
        {
            // Initialize any setup code here if needed
        }

        [Benchmark]
        public string FiveCharacterString()
        {
            return FiveCharacterStringCreate(_getRandom5LetterString).Value;

        }

        [Benchmark]
        public string ConcatenationOperator()
        {
            return _getRandom5LetterString() + _getRandom5LetterString() + _getRandom5LetterString() + _getRandom5LetterString() + _getRandom5LetterString();
        }

        [Benchmark]
        public string StringInterpolation()
        {
            return $"{_getRandom5LetterString()}{_getRandom5LetterString()}{_getRandom5LetterString()}{_getRandom5LetterString()}{_getRandom5LetterString()}";
        }

    }
}
