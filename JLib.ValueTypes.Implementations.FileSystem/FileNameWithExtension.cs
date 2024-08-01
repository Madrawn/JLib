using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLib.ValueTypes.Definitions.FileSystem;

public interface IPathSegment;
public interface IPath;

public record FileNameWithExtension(string Value) : StringValueType(Value), IPathSegment
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar)
            .Contain('.');
}
public record FileNameWithoutExtension(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar);

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
public record FileExtension(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotStartWith('.')
            .NotContain(Path.GetInvalidFileNameChars());
}

/// <summary>
/// the name of a single directory in a path, not a <see cref="RelativeDirectoryPath"/>, <see cref="AbsoluteDirectoryPath"/> or <see cref="DriveLetter"/>
/// </summary>
/// <param name="Value"></param>
public record DirectoryName(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(Path.GetInvalidPathChars())
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar);
}
public record RelativeDirectoryPath(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContain(':')
            .NotContain(Path.DirectorySeparatorChar)
            .NotContain(Path.AltDirectorySeparatorChar);
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