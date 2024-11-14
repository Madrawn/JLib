using JLib.Helper;

// ReSharper disable UnusedMember.Local

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// extension methods for validating file paths
/// </summary>
public static class FilePathValidationExtensions
{
    /// <summary>
    /// checks for <see cref="Path.IsPathRooted(ReadOnlySpan{char})"/> = true
    /// </summary>
    public static IValidationContext<string?> BeRootPath(this IValidationContext<string?> context)
    {
        if (Path.IsPathRooted(context.Value) is false)
            context.AddError("Must be rooted");
        return context;
    }
    /// <summary>
    /// checks for <see cref="Path.IsPathRooted(ReadOnlySpan{char})"/> = false
    /// </summary>
    public static IValidationContext<string?> BeRelativePath(this IValidationContext<string?> context)
    {
        if (Path.IsPathRooted(context.Value))
            context.AddError("Must not be rooted");
        return context;
    }

    private static readonly string InvalidPathChars = string.Join(", ", Path.GetInvalidPathChars());
    /// <summary>
    /// checks whether the value contains <see cref="Path.GetInvalidPathChars"/>
    /// </summary>
    public static IValidationContext<string?> NotContainInvalidPathChars(this IValidationContext<string?> context)
        => context.SatisfyCondition(x => Path.GetInvalidPathChars().Contains(x) is false,
            $"Must not contain invalid path chars ({InvalidPathChars})");

    private static readonly string InvalidFileNameChars = string.Join(", ", Path.GetInvalidFileNameChars());
    /// <summary>
    /// checks whether the value contains <see cref="Path.GetInvalidFileNameChars"/>
    /// </summary>
    public static IValidationContext<string?> NotContainInvalidFileNameChars(this IValidationContext<string?> context)
        => context.SatisfyCondition(x => Path.GetInvalidFileNameChars().Contains(x) is false,
            $"Must not contain invalid path chars ({InvalidFileNameChars})");

    /// <summary>
    /// checks for <see cref="Path.HasExtension(ReadOnlySpan{char})"/> = true
    /// </summary>
    public static IValidationContext<string?> HaveAnExtension(this IValidationContext<string?> context)
    {
        if (Path.HasExtension(context.Value) is false)
            context.AddError("Must have an extension");
        return context;
    }
    /// <summary>
    /// checks for <see cref="Path.GetExtension(ReadOnlySpan{char})"/> = <paramref name="extension"/>
    /// </summary>
    public static IValidationContext<string?> HaveExtension(this IValidationContext<string?> context, FileExtension extension)
    {
        if (Path.GetExtension(context.Value) != extension.Value)
            context.AddError($"Must have the '{extension.Value}' extension but has '{extension.Value}'");
        return context;
    }
    /// <summary>
    /// checks for <see cref="Path.HasExtension(ReadOnlySpan{char})"/> = false
    /// </summary>
    public static IValidationContext<string?> HaveNoExtension(this IValidationContext<string?> context)
    {
        if (Path.HasExtension(context.Value) is false)
            context.AddError("Must have no extension");
        return context;
    }
    public static IValidationContext<string?> HaveAnDirectory(this IValidationContext<string?> context)
    {
        if (Path.GetDirectoryName(context.Value) is null)
            context.AddError($"Must have an directory");
        return context;
    }
}

/// <summary>
/// Identifies, the implementing ValueType as part of a filepath
/// </summary>
public interface IPathSegment { }

/// <summary>
/// any valid filename without path information, but with and file extension<br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not contain <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must contain '.'
/// </summary>
/// <param name="Value">the filename</param>
public record FileNameWithExtension(string Value) : StringValueType(Value), IPathSegment
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContainInvalidFileNameChars()
            .HaveAnExtension();

    /// <returns>the <see cref="FileExtension"/></returns>
    public FileExtension GetExtension()
        => new(Path.GetExtension(Value).TrimStart('.'));

    /// <returns>the <see cref="FileNameWithoutExtension"/></returns>
    public FileNameWithoutExtension RemoveExtension()
        => new(Path.GetFileNameWithoutExtension(Value));
}
/// <summary> 
/// any valid filename with path information and file extension<br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not contain <see cref="Path.GetInvalidFileNameChars"/><br/>
/// </summary>
/// <param name="Value">the filename</param>
public record FileNameWithoutExtension(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContainInvalidFileNameChars()
            .HaveNoExtension();

    /// <summary>
    /// appends the <paramref name="extension"/> to the <paramref name="name"/>. If the extension is empty, the name is returned as is.
    /// </summary>
    public static FileNameWithExtension operator +(FileNameWithoutExtension name, FileExtension extension)
        => new(
            extension.Value == ""
            ? name.Value
            : $"{name.Value}.{extension.Value}"
        );
}

/// <summary>
/// any valid file extension<br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not contain <see cref="Path.GetInvalidFileNameChars"/><br/>
/// must not start with '.'
/// </summary>
/// <param name="Value">the file extension</param>
public record FileExtension(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotStartWith('.')
            .NotContainInvalidFileNameChars();
}
/// <summary>
/// the name of a single directory in a path, not a <see cref="RelativeDirectoryPath"/>, <see cref="AbsoluteDirectoryPath"/> or <see cref="DriveLetter"/><br/><br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidPathChars"/><br/><br/>
/// must not contain <see cref="Path.GetInvalidPathChars"/><br/>
/// must not contain <see cref="Path.DirectorySeparatorChar"/><br/>
/// must not contain <see cref="Path.AltDirectorySeparatorChar"/><br/>
/// </summary>
/// <param name="Value">the directory name</param>
public record DirectoryName(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(Path.GetInvalidPathChars())
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar);
}

/// <summary>
/// a relative directory path <see cref="AbsoluteDirectoryPath"/>. It may contain only one directory and may use relative navigation<br/><br/>
/// Validation may differ between operating systems due to the usage of <see cref="Path.GetInvalidPathChars"/>
/// <remarks>
/// <br/><br/>
/// <see cref="Path.IsPathRooted(ReadOnlySpan{char})"/> must evaluate <paramref name="Value"/> to false
/// must not contain <see cref="Path.GetInvalidPathChars"/><br/>
/// may contain <see cref="Path.DirectorySeparatorChar"/><br/>
/// may contain <see cref="Path.AltDirectorySeparatorChar"/><br/>
/// </remarks>
/// </summary>
/// <param name="Value">the directory name</param>
public record RelativeDirectoryPath(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> context)
    {
        if (Path.IsPathRooted(context.Value))
            context.AddError("The path must not be rooted");
        context.NotContain(Path.GetInvalidPathChars());
    }
    /// <returns>the directory which contains this directory</returns>
    public RelativeDirectoryPath? GetParent()
        => ValueType.CreateNullable<RelativeDirectoryPath, string>(Path.GetDirectoryName(Value));
    /// <returns>the name of the current directory</returns>
    public DirectoryName GetCurrent()
        => new(Path.GetFileName(Value));
}
/// <summary>
/// a rooted path to a directory which may or may not exist
/// </summary>
public record AbsoluteDirectoryPath(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .BeRootPath()
            .NotContainInvalidPathChars();

    /// <summary>
    /// appends the <paramref name="relativePath"/> to the <param name="path"/> and returns the resulting <see cref="AbsoluteDirectoryPath"/>
    /// </summary>
    public static AbsoluteDirectoryPath operator +(AbsoluteDirectoryPath path, RelativeDirectoryPath relativePath)
        => new(Path.Combine(path.Value, relativePath.Value));

    /// <summary>
    /// returns
    /// <remarks>
    /// <example>
    /// <code>@"a\b\x\y" - @"a\b" => @"x\y"</code>
    /// </example>
    /// </remarks>
    /// </summary>
    public static RelativeDirectoryPath operator -(AbsoluteDirectoryPath path, AbsoluteDirectoryPath absolutePathTobeRemoved)
        => new(Path.GetRelativePath(absolutePathTobeRemoved.Value, path.Value));


    /// <summary>
    /// appends the <paramref name="file"/> to the <param name="path"/> and returns the resulting <see cref="AbsoluteFilePath"/>
    /// </summary>
    public static AbsoluteFilePath operator +(AbsoluteDirectoryPath path, FileNameWithExtension file)
        => new(Path.Combine(path.Value, file.Value));

    /// <summary>
    /// appends the <paramref name="filePath"/> to the <param name="path"/> and returns the resulting <see cref="AbsoluteFilePath"/>
    /// </summary>
    public static AbsoluteFilePath operator +(AbsoluteDirectoryPath path, RelativeFilePath filePath)
        => new(Path.Combine(path.Value, filePath.Value));

    /// <returns>All files contained in this directory</returns>
    public IReadOnlyCollection<AbsoluteFilePath> GetFiles()
    => Directory.GetFiles(Value).Select(x => new AbsoluteFilePath(x)).ToReadOnlyCollection();

    /// <returns>All Subdirectories of this directory</returns>
    public IReadOnlyCollection<AbsoluteDirectoryPath> GetDirectories()
        => Directory.GetDirectories(Value).Select(x => new AbsoluteDirectoryPath(x)).ToReadOnlyCollection();

    /// <returns>The directory which contains this directory</returns>
    public AbsoluteDirectoryPath? GetParent()
        => ValueType.CreateNullable<AbsoluteDirectoryPath, string>(Path.GetDirectoryName(Value));
    /// <returns>The name of the current directory</returns>
    public DirectoryName GetCurrent()
        => new(Path.GetFileName(Value));
    /// <returns>whether this directory exists or not</returns>
    public bool Exists()
        => Directory.Exists(Value);
    /// <summary>
    /// Creates this directory
    /// </summary>
    public void Create() => Directory.CreateDirectory(Value);
}
/// <summary>
/// The letter of a windows filesystem drive, e.g. 'C'
/// </summary>
public record DriveLetter(char Value) : CharValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<char> must)
        => must.BeAsciiLetter();

    /// <summary>
    /// appends the <paramref name="path"/> to the <paramref name="letter"/>, making it absolute
    /// </summary>
    public static AbsoluteDirectoryPath operator +(DriveLetter letter, RelativeDirectoryPath path)
        => new($"{letter.Value}:{Path.DirectorySeparatorChar}{path.Value}");
    /// <summary>
    /// appends the <paramref name="filePath"/> to the <paramref name="letter"/>, making it absolute
    /// </summary>
    public static AbsoluteDirectoryPath operator +(DriveLetter letter, RelativeFilePath filePath)
        => new($"{letter.Value}:{Path.DirectorySeparatorChar}{filePath.Value}");
}