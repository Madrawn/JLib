using System.Security.AccessControl;
using Microsoft.Win32.SafeHandles;

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// an absolute path to a file which may or may not exist
/// </summary>
public record AbsoluteFilePath(string Value) : StringValueType(Value)
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .BeRootPath()
            .NotContain(Path.GetInvalidFileNameChars())
            .HaveAnDirectory();

    /// <returns>the directory of this path</returns>
    public AbsoluteDirectoryPath GetDirectory()
        => new(Path.GetDirectoryName(Value)!);// guaranteed to be not null by validation
    /// <returns>the filename of this path</returns>-
    public FileNameWithExtension GetFileName()
        => new(Path.GetFileName(Value));
    /// <returns>whether this file exists or not</returns>
    public bool Exists()
        => File.Exists(Value);

    #region data operations

    #region stream operations
    public FileStream Open(FileMode mode) => File.Open(Value, mode);
    public FileStream OpenRead() => File.OpenRead(Value);
    public FileStream OpenWrite() => File.OpenWrite(Value);
    public StreamReader OpenText() => File.OpenText(Value);
    public SafeFileHandle OpenHandle() => File.OpenHandle(Value);
    public FileStream Create() => File.Create(Value);
    public StreamWriter CreateText() => File.CreateText(Value);

    public StreamWriter AppendText() => File.AppendText(Value);
    #endregion
    public FileSystemInfo CreateSymbolicLink(AbsoluteFilePath targetPath) => File.CreateSymbolicLink(Value, targetPath.Value);
    public FileSystemInfo? ResolveLinkTarget(bool returnFinalTarget) => File.ResolveLinkTarget(Value, returnFinalTarget);
    public void Decrypt() => File.Decrypt(Value);
    public void Encrypt() => File.Encrypt(Value);
    #region set attributes
    public void SetAttributes(FileAttributes attributes) => File.SetAttributes(Value, attributes);
    public void SetCreationTime(DateTime creationTime) => File.SetCreationTime(Value, creationTime);
    public void SetCreationTimeUtc(DateTime creationTimeUtc) => File.SetCreationTimeUtc(Value, creationTimeUtc);
    public void SetLastAccessTime(DateTime lastAccessTime) => File.SetLastAccessTime(Value, lastAccessTime);
    public void SetLastAccessTimeUtc(DateTime lastAccessTimeUtc) => File.SetLastAccessTimeUtc(Value, lastAccessTimeUtc);
    public void SetLastWriteTime(DateTime lastWriteTime) => File.SetLastWriteTime(Value, lastWriteTime);
    public void SetLastWriteTimeUtc(DateTime lastWriteTimeUtc) => File.SetLastWriteTimeUtc(Value, lastWriteTimeUtc);
    #endregion

    #region get attributes
    public FileAttributes GetAttributes() => File.GetAttributes(Value);
    public DateTime GetCreationTime() => File.GetCreationTime(Value);
    public DateTime GetCreationTimeUtc() => File.GetCreationTimeUtc(Value);
    public DateTime GetLastAccessTime() => File.GetLastAccessTime(Value);
    public DateTime GetLastAccessTimeUtc() => File.GetLastAccessTimeUtc(Value);
    public DateTime GetLastWriteTime() => File.GetLastWriteTime(Value);
    public DateTime GetLastWriteTimeUtc() => File.GetLastWriteTimeUtc(Value);
    #endregion





    #region write
    /// <summary>
    /// creates this directory
    /// </summary>
    public void CreateDirectory() => GetDirectory()?.Create();
    public void WriteAllText(string text) => File.WriteAllText(Value, text);
    public Task WriteAllTextAsync(string text) => File.WriteAllTextAsync(Value, text);
    public void WriteAllBytes(byte[] bytes) => File.WriteAllBytes(Value, bytes);
    public Task WriteAllBytesAsync(byte[] bytes) => File.WriteAllBytesAsync(Value, bytes);
    public void WriteAllLines(string[] lines) => File.WriteAllLines(Value, lines);
    public Task WriteAllLinesAsync(string[] lines) => File.WriteAllLinesAsync(Value, lines);
    #endregion

    #region append
    public void AppendAllText(string text) => File.AppendAllText(Value, text);
    public Task AppendAllTextAsync(string text) => File.AppendAllTextAsync(Value, text);
    public void AppendAllLines(string[] lines) => File.AppendAllLines(Value, lines);
    public Task AppendAllLinesAsync(string[] lines) => File.AppendAllLinesAsync(Value, lines);
    #endregion

    #region read
    public string ReadAllText() => File.ReadAllText(Value);
    public Task<string> ReadAllTextAsync() => File.ReadAllTextAsync(Value);
    public byte[] ReadAllBytes() => File.ReadAllBytes(Value);
    public Task<byte[]> ReadAllBytesAsync() => File.ReadAllBytesAsync(Value);
    public string[] ReadAllLines() => File.ReadAllLines(Value);
    public Task<string[]> ReadAllLinesAsync() => File.ReadAllLinesAsync(Value);
    #endregion

    #endregion

    #region fileSystem operations
    public void CopyTo(AbsoluteFilePath destination) => File.Copy(Value, destination.Value);
    public void CopyTo(AbsoluteFilePath destination, bool overwrite) => File.Copy(Value, destination.Value, overwrite);
    public void MoveTo(AbsoluteFilePath destination) => File.Move(Value, destination.Value);
    public void Rename(FileNameWithExtension newName) => MoveTo(GetDirectory() + newName);
    public void Delete() => File.Delete(Value);

    #endregion
}