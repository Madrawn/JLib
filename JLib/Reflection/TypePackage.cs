using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using JLib.Helper;

namespace JLib.Reflection;

/// <summary>
/// Contains types to be used by the <see cref="TypeCache"/>
/// </summary>
public interface ITypePackage
{
    /// <summary>
    /// The <see cref="Types"/> of this package and all its <see cref="Children"/>
    /// </summary>
    public ImmutableHashSet<Type> GetContent();
    /// <summary>
    /// other nested packages, this could be peer dependencies, the assemblies of a directory or any other group of packages
    /// </summary>
    public IEnumerable<ITypePackage> Children { get; }

    /// <summary>
    /// The types which are directly defined in this package.
    /// </summary>
    public IEnumerable<Type> Types { get; }

    /// <summary>
    /// a short description of the contents of the type package.<br/>
    /// {0} is replaced by the number of <see cref="Children"/><br/>
    /// {1} is replaced by the number of <see cref="Types"/>
    /// </summary>
    public string DescriptionTemplate { get; }

    /// <summary>
    /// returns a type package which is a combination of the given packages
    /// </summary>
    public ITypePackage Combine(params ITypePackage[] packages);

    public string ToString(bool includeTypes);
}

/// <summary>
/// Contains Content to be used by the <see cref="TypeCache"/>
/// </summary>
public class TypePackage : ITypePackage
{
    public static ITypePackage Get(Assembly assembly, string? name = null)
        => new TypePackage(assembly.GetTypes(), null, name ?? assembly.FullName ?? "Nameless Assembly with {0} Types");

    public static ITypePackage Get(params Assembly[] assemblies)
        => Get(assemblies.CastTo<IReadOnlyCollection<Assembly>>());

    public static ITypePackage Get(IReadOnlyCollection<Assembly> assemblies)
        => new TypePackage(null, assemblies.Select(a => Get(a)), "{1} Assemblies");

    public static ITypePackage Get(params Type[] types)
        => Get(types.CastTo<IReadOnlyCollection<Type>>());

    public static ITypePackage Get(IReadOnlyCollection<Type> types)
        => new TypePackage(types, null, "{1} Types");

    /// <summary>
    /// creates a <see cref="ITypePackage"/> which contains all types nested in the given types, but not the types themselves.<br/>
    /// this can be usefull for testing purposes
    /// </summary>
    public static ITypePackage GetNested(params Type[] types)
        => new TypePackage(types.SelectMany(x => x.GetNestedTypes()), null,
            "nested types of " + string.Join(", ", types.Select(x => x.FullClassName())));

    /// <summary>
    /// creates a <see cref="ITypePackage"/> which contains all types nested in <typeparamref nameTemplate="T"/> but not <typeparamref nameTemplate="T"/> itself.
    /// </summary>
    public static ITypePackage GetNested<T>()
        => GetNested(typeof(T));

    public static ITypePackage Get(IEnumerable<Assembly> assemblies, IEnumerable<Type> types,
        string name = "{0} Assemblies and {1} types")
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
        DescriptionTemplate = nameTemplate;
    }


    public IEnumerable<ITypePackage> Children { get; }
    public IEnumerable<Type> Types { get; }
    public string DescriptionTemplate { get; }
    public ImmutableHashSet<Type> GetContent() => GetContent(this).ToImmutableHashSet();

    private static IEnumerable<Type> GetContent(ITypePackage package) 
        => package.Children.SelectMany(GetContent).Concat(package.Types).Distinct();

    public ITypePackage Combine(params ITypePackage[] packages)
        => Get(packages.Append(this));

    public override string ToString()
        => ToString(false);

    public string ToString(bool includeTypes)
    {
        var sb = new StringBuilder();
        ToString(this, 0, sb, includeTypes);
        return sb.ToString();
    }

    static void ToString(ITypePackage package, int indent, StringBuilder sb, bool includeTypes)
    {
        var indentStr = new string(' ', indent * 2);
        sb.Append(indentStr).Append('┐').AppendLine(string.Format(package.DescriptionTemplate, package.Children.Count(),
            package.Types.Count()));
        sb.Append(indentStr).Append("├ Types:").AppendLine(package.Types.Count().ToString());
        if (package.Types.Count() <= 10 || includeTypes)
        {
            foreach (var type in package.Types)
                sb.Append(indentStr).Append("│   ").AppendLine(type.FullClassName());
        }
        if (package.Children.Any())
        {
            sb.Append(indentStr).AppendLine("├ Children:");
            foreach (var child in package.Children)
                ToString(child, indent + 1, sb, includeTypes);
        }
    }
}