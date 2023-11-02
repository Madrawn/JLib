using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// Contains Content to be used by the <see cref="TypeCache"/>
/// </summary>
public interface ITypePackage
{
    public IEnumerable<Type> Content { get; }
    public IEnumerable<ITypePackage> Children { get; }
    public IEnumerable<Type> Types { get; }
    public string NameTemplate { get; }

    public ITypePackage Combine(params ITypePackage[] packages);
}
/// <summary>
/// Contains Content to be used by the <see cref="TypeCache"/>
/// </summary>
public class TypePackage : ITypePackage
{
    public static ITypePackage Get(Assembly assembly, string? name)
        => new TypePackage(assembly.GetTypes(), null, name ?? assembly.FullName ?? "Nameless Assembly with {0} Types");

    public static ITypePackage Get(Assembly assembly)
        => Get(assembly, (string?)null);
    public static ITypePackage Get(params Assembly[] assemblies)
        => new TypePackage(null,assemblies.Select(Get),"{1} Assemblies");
    public static ITypePackage Get(params Type[] types)
        => new TypePackage(types, null, "{1} Types");
    /// <summary>
    /// creates a <see cref="ITypePackage"/> which contains all types nested in the given types, but not the types themselves.
    /// </summary>
    public static ITypePackage GetNested(params Type[] types)
        => new TypePackage(types.SelectMany(x => x.GetNestedTypes()), null, "nested types of " + string.Join(", ", types.Select(x => x.FullClassName())));

    /// <summary>
    /// creates a <see cref="ITypePackage"/> which contains all types nested in <typeparamref nameTemplate="T"/> but not <typeparamref nameTemplate="T"/> itself.
    /// </summary>
    public static ITypePackage GetNested<T>()
        => GetNested(typeof(T));
    public static ITypePackage Get(IEnumerable<Assembly> assemblies, IEnumerable<Type> types, string name = "{0} Assemblies and {1} types")
        => new TypePackage(types, assemblies.Select(a => Get(a)), name);

    public static ITypePackage Get(params ITypePackage[] packages)
        => Get(packages.AsEnumerable());
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static ITypePackage Get(IEnumerable<ITypePackage> packages, string name = "{0} Packages")
        => packages.Multiple()
            ? new TypePackage(
                null,
                packages,
                name)
            : packages.Single();

    /// <summary>
    /// creates a <see cref="ITypePackage"/> which contains all types of all assemblies in the given directory which match any of the given prefixes.
    /// </summary>
    /// <param name="directory">defaults to <see cref="AppDomain.CurrentDomain"/>.BaseDirectory</param>
    /// <param name="includedPrefixes"></param>
    /// <param name="searchOption"></param>
    /// <returns></returns>
    public static ITypePackage Get(
        string? directory,
        string[] includedPrefixes,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {

        directory ??= AppDomain.CurrentDomain.BaseDirectory;
        var assemblyNames = Directory.EnumerateFiles(directory, "*.dll", searchOption).Where(file =>
        {
            var filename = Path.GetFileName(file);
            return includedPrefixes.Any(p => filename.StartsWith(p));
        }).Select(AssemblyName.GetAssemblyName).ToArray();

        var assemblies = assemblyNames.Select(Assembly.Load).ToArray();
        return Get(assemblies);
    }
    private TypePackage(IEnumerable<Type>? types, IEnumerable<ITypePackage>? children, string nameTemplate)
    {
        Types = types ?? Enumerable.Empty<Type>();
        Children = children ?? Array.Empty<ITypePackage>();
        NameTemplate = nameTemplate;
    }


    public IEnumerable<ITypePackage> Children { get; }
    public IEnumerable<Type> Types { get; }
    public string NameTemplate { get; }
    public IEnumerable<Type> Content => Children.SelectMany(x => x.Content).Concat(Types);
    public ITypePackage Combine(params ITypePackage[] packages)
        => Get(packages.Append(this));

    public override string ToString()
    {
        var sb = new StringBuilder();
        ToString(this, 0, sb);
        return sb.ToString();
    }

    static void ToString(ITypePackage package, int indent, StringBuilder sb)
    {
        var indentStr = new string(' ', indent * 2);
        sb.Append(indentStr).Append('┐').AppendLine(string.Format(package.NameTemplate, package.Children.Count(), package.Types.Count()));
        sb.Append(indentStr).Append("├ Types:").AppendLine(package.Types.Count().ToString());
        sb.Append(indentStr).Append("├ Types Total:").AppendLine(package.Content.Count().ToString());
        sb.Append(indentStr).AppendLine("├ Children:");
        foreach (var child in package.Children)
            ToString(child, indent + 1, sb);

    }
}