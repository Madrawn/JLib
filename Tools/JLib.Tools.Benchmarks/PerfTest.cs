using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using HarmonyLib;

using JLib.Exceptions;
using JLib.ValueTypes;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace JLib.Tools.Benchmarks;

public class PerfTest
{
    private static readonly Func<string> _getRandom5LetterString = GetNextString;

    //[Config(typeof(Config))]
    private static readonly List<string> _strings = Enumerable.Range(0, 5000).Select(_ => GetRandom5LetterString()).ToList();

    private static IEnumerator<string> _stringEnumerator = _strings.GetEnumerator();
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

    public static FiveCharacterString FiveCharacterStringCreate(Func<string> rand) => ValueTypes.ValueType.Create<FiveCharacterString, string>(rand());

    public static string GetRandom5LetterString()
    {
        var str = random.Next(10000, 99999).ToString();
        return $"{str}";
    }

    public static Func<int> GetRandomPositiveInt(Random random) => () => random.Next(1, short.MaxValue);

    private static string GetNextString()
    {
        if (!_stringEnumerator.MoveNext())
        {
            _stringEnumerator = _strings.GetEnumerator();
            _stringEnumerator.MoveNext();
        }
        return _stringEnumerator.Current;
    }

    public class CheckPerf
    {
        [Params(5, 0)]
        public int val { get; set; } = 5;

        [Benchmark]
        public bool[] EqualityCheck()
        {
            return Enumerable.Range(0, 1000).Select((x) => val == x).ToArray();
        }

        [Benchmark]
        public bool[] InEqualityCheck()
        {
            return Enumerable.Range(0, 1000).Select(x => val > x).ToArray();
        }
    }

    public class Config : ManualConfig
    {
        public Config()
        {
            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }

    public class StringPerformanceBenchmarks
    {
        [Benchmark]
        public string ConcatenationOperator()
        {
            return _getRandom5LetterString() + _getRandom5LetterString() + _getRandom5LetterString() + _getRandom5LetterString() + _getRandom5LetterString();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Initialize any setup code here if needed
        }

        [Benchmark]
        public string StringInterpolation()
        {
            return $"{_getRandom5LetterString()}{_getRandom5LetterString()}{_getRandom5LetterString()}{_getRandom5LetterString()}{_getRandom5LetterString()}";
        }
    }

    // Crimes against god here
    public class Swapper
    {
        // Create a Harmony instance
        private static Harmony harmony = new Harmony("com.example.patch");

        // Get the method to patch
        private static MethodInfo originalMethod = typeof(ExceptionBuilder)!.GetMethod("GetException")!;

        [Params("patch", "lockedpatch", "unpatched")]
        public string? PatchState { get; set; }

        [Benchmark]
        public string FiveCharacterString()
        {
            return FiveCharacterStringCreate(_getRandom5LetterString).Value;
        }

        [GlobalSetup(Target = nameof(FiveCharacterString))]
        public void GlobalSetup2()
        {
            harmony.Unpatch(originalMethod, HarmonyPatchType.All, "com.example.patch");
            if (PatchState == "patch")
            {
                harmony.Patch(originalMethod, new HarmonyMethod(typeof(PatchClass).GetMethod(nameof(PatchClass.Prefix))!));
            }
            if (PatchState == "lockedpatch")
            {
                harmony.Patch(originalMethod, new HarmonyMethod(typeof(PatchClass).GetMethod(nameof(PatchClass.LockedPrefix))!));
            }
        }

        // #region Experimental Patching
        public class PatchClass
        {
            public static bool LockedPrefix(List<Exception> ____exceptions, List<IExceptionProvider> ____children, object ____exceptionLock, object ____childrenLock)
            {
                lock (____exceptionLock)
                    lock (____childrenLock)
                        return Prefix(____exceptions, ____children);
            }

            public static bool Prefix(List<Exception> ____exceptions, List<IExceptionProvider> ____children)
            {
                bool condition = ____exceptions.Count == 0 && ____children.Count == 0;
                if (condition)
                {
                    return false; // Returning false skips the original method
                }
                return true; // Returning true allows the original method to execute
            }
        }
    }
}