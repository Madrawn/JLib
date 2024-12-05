using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace JLib.Tools.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        string? projectPath = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly,
            ManualConfig
                    .Create(DefaultConfig.Instance)
                    .WithArtifactsPath(Path.Combine(projectPath??".\\", "BenchmarkDotNet.Artifacts")));
    }
}
