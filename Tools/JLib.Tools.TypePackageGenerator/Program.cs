

using System.Diagnostics.Contracts;

Console.WriteLine("Type Package Generator");
Console.WriteLine("This code generates type packages for all projects in the given solution. Namespace fixes and class removal might be required afterwards");
string? solutionDir;
do
{
    Console.WriteLine("Solution Directory: ");
    solutionDir = Console.ReadLine();
} while (solutionDir == null || !Directory.Exists(solutionDir));

var projFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);
foreach (var projFile in projFiles)
{
    var projectDir = Path.GetDirectoryName(projFile);
    var projName = Path.GetFileNameWithoutExtension(projFile);

    var @namespace = projName;
    var className = projName.Replace(".", "").Replace(" ", "") + "Tp";
    var fileName = $"{className}.cs";

    var filePath = fileName;
    if (projectDir != null)
    {
        filePath = Path.Combine(projectDir, fileName);
    }

    if (File.Exists(filePath))
    {
        Console.WriteLine($"file found. Skipping file generation for {filePath}");
        continue;
    }

    try
    {
        Console.WriteLine($"generating file {filePath}");
        File.WriteAllText(filePath, $@"
using JLib.Reflection;

namespace {@namespace};
public static class {className}
{{
    public static ITypePackage Instance {{ get; }} = TypePackage.Get(typeof({className}).Assembly);
}}
");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine("failed:");
        Console.Error.WriteLine(e);
    }

}
