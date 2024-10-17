namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// a relative path to a file which may or may not exist
/// </summary>
public record RelativeFilePath(string Value) : FilePath(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .BeRelativePath()
            .NotContainInvalidFileNameChars();
}