namespace JLib.ValueTypes.Implementations.FileSystem;

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
            .NotContain(Path.GetInvalidFileNameChars())
            .Contain('.');
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
            .NotContain(Path.GetInvalidFileNameChars());

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
            .NotContain(Path.GetInvalidFileNameChars());
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
}
public record AbsoluteDirectoryPath(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar)
            .BeAscii()
            .BeAlphanumeric();
    public static AbsoluteDirectoryPath operator +(AbsoluteDirectoryPath path, RelativeDirectoryPath relativePath)
        => new(Path.Combine(path.Value, relativePath.Value));
}
public record DriveLetter(char Value) : CharValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<char> must)
        => must.BeAsciiLetter();

    public static AbsoluteDirectoryPath operator +(DriveLetter letter, RelativeDirectoryPath path)
        => new($"{letter.Value}:{Path.DirectorySeparatorChar}{path.Value}");
}