using BenchmarkDotNet.Attributes;

using HarmonyLib;

using JLib.Exceptions;

using System.Reflection;

namespace JLib.Tools.Benchmarks;

public partial class PerfTest
{
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