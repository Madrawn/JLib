using System.Text;
using Microsoft.Win32.SafeHandles;

namespace JLib.ValueTypes.Implementations.FileSystem;

/// <summary>
/// the path to a file, it might be either <see cref="AbsoluteFilePath"/> or <see cref="RelativeFilePath"/>
/// </summary>
/// <param name="Value"></param>
public abstract record FilePath(string Value) : StringValueType(Value)
{
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
    /// <summary>
    /// <inheritdoc cref="File.Open(string, FileMode)"/>
    /// </summary>
    public FileStream Open(FileMode mode) => File.Open(Value, mode);
    /// <summary>
    /// <inheritdoc cref="File.Open(string, FileMode, FileAccess)"/>
    /// </summary>
    public FileStream Open(FileMode mode, FileAccess access) => File.Open(Value, mode, access);
    /// <summary>
    /// <inheritdoc cref="File.Open(string, FileMode, FileAccess, FileShare)"/>
    /// </summary>
    public FileStream Open(FileMode mode, FileAccess access, FileShare share) => File.Open(Value, mode, access, share);
    /// <summary>
    /// <inheritdoc cref="File.Open(string, FileStreamOptions)"/>
    /// </summary>
    public FileStream Open(FileStreamOptions streamOptions) => File.Open(Value, streamOptions);
    /// <summary>
    /// <inheritdoc cref="File.OpenRead"/>
    /// </summary>
    public FileStream OpenRead() => File.OpenRead(Value);
    /// <summary>
    /// <inheritdoc cref="File.OpenWrite"/>
    /// </summary>
    public FileStream OpenWrite() => File.OpenWrite(Value);
    /// <summary>
    /// <inheritdoc cref="File.OpenText"/>
    /// </summary>
    public StreamReader OpenText() => File.OpenText(Value);
    /// <summary>
    /// <inheritdoc cref="File.OpenHandle"/>
    /// </summary>
    public SafeFileHandle OpenHandle(
        FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read,
        FileOptions options = FileOptions.None, long preAllocationSize = 0L)
        => File.OpenHandle(Value, mode, access, share, options, preAllocationSize);
    /// <summary>
    /// <inheritdoc cref="File.Create(string)"/>
    /// </summary>
    public FileStream Create() => File.Create(Value);
    /// <summary>
    /// <inheritdoc cref="File.Create(string,int)"/>
    /// </summary>
    public FileStream Create(int bufferSize) => File.Create(Value, bufferSize);
    /// <summary>
    /// <inheritdoc cref="File.Create(string,int,FileOptions)"/>
    /// </summary>
    public FileStream Create(int bufferSize, FileOptions options) => File.Create(Value, bufferSize, options);
    /// <summary>
    /// <inheritdoc cref="File.CreateText"/>
    /// </summary>
    public StreamWriter CreateText() => File.CreateText(Value);

    /// <summary>
    /// <inheritdoc cref="File.AppendText"/>
    /// </summary>
    public StreamWriter AppendText() => File.AppendText(Value);
    #endregion
    /// <summary>
    /// <inheritdoc cref="File.CreateSymbolicLink"/>
    /// </summary>
    public FileSystemInfo CreateSymbolicLink(AbsoluteFilePath targetPath) => File.CreateSymbolicLink(Value, targetPath.Value);
    /// <summary>
    /// <inheritdoc cref="File.ResolveLinkTarget"/>
    /// </summary>
    public FileSystemInfo? ResolveLinkTarget(bool returnFinalTarget) => File.ResolveLinkTarget(Value, returnFinalTarget);

    #region set attributes
    /// <summary>
    /// <inheritdoc cref="File.SetAttributes"/>
    /// </summary>
    public void SetAttributes(FileAttributes attributes) => File.SetAttributes(Value, attributes);
    /// <summary>
    /// <inheritdoc cref="File.C"/>
    /// </summary>
    public void SetCreationTime(DateTime creationTime) => File.SetCreationTime(Value, creationTime);
    /// <summary>
    /// <inheritdoc cref="File.SetCreationTimeUtc"/>
    /// </summary>
    public void SetCreationTimeUtc(DateTime creationTimeUtc) => File.SetCreationTimeUtc(Value, creationTimeUtc);
    /// <summary>
    /// <inheritdoc cref="File.SetLastAccessTime"/>
    /// </summary>
    public void SetLastAccessTime(DateTime lastAccessTime) => File.SetLastAccessTime(Value, lastAccessTime);
    /// <summary>
    /// <inheritdoc cref="File.SetLastAccessTimeUtc"/>
    /// </summary>
    public void SetLastAccessTimeUtc(DateTime lastAccessTimeUtc) => File.SetLastAccessTimeUtc(Value, lastAccessTimeUtc);
    /// <summary>
    /// <inheritdoc cref="File.SetLastWriteTime"/>
    /// </summary>
    public void SetLastWriteTime(DateTime lastWriteTime) => File.SetLastWriteTime(Value, lastWriteTime);
    /// <summary>
    /// <inheritdoc cref="File.SetLastWriteTimeUtc"/>
    /// </summary>
    public void SetLastWriteTimeUtc(DateTime lastWriteTimeUtc) => File.SetLastWriteTimeUtc(Value, lastWriteTimeUtc);

    #endregion

    #region get attributes
    /// <summary>
    /// <inheritdoc cref="File.GetAttributes"/>
    /// </summary>
    public FileAttributes GetAttributes() => File.GetAttributes(Value);
    /// <summary>
    /// <inheritdoc cref="File.GetCreationTime"/>
    /// </summary>
    public DateTime GetCreationTime() => File.GetCreationTime(Value);
    /// <summary>
    /// <inheritdoc cref="File.GetCreationTimeUtc"/>
    /// </summary>
    public DateTime GetCreationTimeUtc() => File.GetCreationTimeUtc(Value);
    /// <summary>
    /// <inheritdoc cref="File.GetLastAccessTime"/>
    /// </summary>
    public DateTime GetLastAccessTime() => File.GetLastAccessTime(Value);
    /// <summary>
    /// <inheritdoc cref="File.GetLastAccessTimeUtc"/>
    /// </summary>
    public DateTime GetLastAccessTimeUtc() => File.GetLastAccessTimeUtc(Value);
    /// <summary>
    /// <inheritdoc cref="File.GetLastWriteTime"/>
    /// </summary>
    public DateTime GetLastWriteTime() => File.GetLastWriteTime(Value);
    /// <summary>
    /// <inheritdoc cref="File.GetLastWriteTimeUtc"/>
    /// </summary>
    public DateTime GetLastWriteTimeUtc() => File.GetLastWriteTimeUtc(Value);
    #endregion





    #region write
    /// <summary>
    /// <inheritdoc cref="File.WriteAllText(string,string)"/>
    /// </summary>
    public void WriteAllText(string? content) => File.WriteAllText(Value, content);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllText(string,string,Encoding)"/>
    /// </summary>
    public void WriteAllText(string? content, Encoding encoding) => File.WriteAllText(Value, content, encoding);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllTextAsync(string,string?,Encoding,CancellationToken)"/>
    /// </summary>
    public Task WriteAllTextAsync(string? content, CancellationToken cancellationToken = default)
        => File.WriteAllTextAsync(Value, content, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllTextAsync(string,string,Encoding, CancellationToken)"/>
    /// </summary>
    public void WriteAllTextAsync(string? content, Encoding encoding, CancellationToken cancellationToken = default)
        => File.WriteAllTextAsync(Value, content, encoding, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllBytes"/>
    /// </summary>
    public void WriteAllBytes(byte[] bytes) => File.WriteAllBytes(Value, bytes);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllBytesAsync"/>
    /// </summary>
    public Task WriteAllBytesAsync(byte[] bytes) => File.WriteAllBytesAsync(Value, bytes);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllLines(string,IEnumerable{string})"/>
    /// </summary>
    public void WriteAllLines(IEnumerable<string> contents) => File.WriteAllLines(Value, contents);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllLines(string,IEnumerable{string}, Encoding)"/>
    /// </summary>
    public void WriteAllLines(IEnumerable<string> contents, Encoding encoding) => File.WriteAllLines(Value, contents, encoding);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllLines(string,string[])"/>
    /// </summary>
    public void WriteAllLines(string[] contents) => File.WriteAllLines(Value, contents);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllLines(string,string[], Encoding)"/>
    /// </summary>
    public void WriteAllLines(string[] contents, Encoding encoding) => File.WriteAllLines(Value, contents, encoding);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllLinesAsync(string,IEnumerable{string},CancellationToken)"/>
    /// </summary>
    public Task WriteAllLinesAsync(IEnumerable<string> contents, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(Value, contents, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.WriteAllLinesAsync(string,IEnumerable{string},Encoding,CancellationToken)"/>
    /// </summary>
    public Task WriteAllLinesAsync(IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default) => File.WriteAllLinesAsync(Value, contents, encoding, cancellationToken);
    #endregion

    #region append
    /// <summary>
    /// <inheritdoc cref="File.AppendAllText(string,string?)"/>
    /// </summary>
    public void AppendAllText(string? text) => File.AppendAllText(Value, text);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllText(string,string?, Encoding)"/>
    /// </summary>
    public void AppendAllText(string? text, Encoding encoding) => File.AppendAllText(Value, text, encoding);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllTextAsync(string,string?,CancellationToken)"/>
    /// </summary>
    public Task AppendAllTextAsync(string? text, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(Value, text, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllTextAsync(string,string?,Encoding,CancellationToken)"/>
    /// </summary>
    public Task AppendAllTextAsync(string? text, Encoding encoding, CancellationToken cancellationToken = default) => File.AppendAllTextAsync(Value, text, encoding, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllLines(string,IEnumerable{string})"/>
    /// </summary>
    public void AppendAllLines(IEnumerable<string> lines) => File.AppendAllLines(Value, lines);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllLines(string,IEnumerable{string}, Encoding)"/>
    /// </summary>
    public void AppendAllLines(IEnumerable<string> lines, Encoding encoding) => File.AppendAllLines(Value, lines, encoding);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllLinesAsync(string,IEnumerable{string},CancellationToken)"/>
    /// </summary>
    public Task AppendAllLinesAsync(IEnumerable<string> lines, CancellationToken cancellationToken = default) => File.AppendAllLinesAsync(Value, lines, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.AppendAllLinesAsync(string,IEnumerable{string},Encoding,CancellationToken)"/>
    /// </summary>
    public Task AppendAllLinesAsync(IEnumerable<string> lines, Encoding encoding, CancellationToken cancellationToken = default) => File.AppendAllLinesAsync(Value, lines, encoding, cancellationToken);
    #endregion

    #region read
    /// <summary>
    /// <inheritdoc cref="File.ReadAllText(string)"/>
    /// </summary>
    public string ReadAllText() => File.ReadAllText(Value);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllText(string)"/>
    /// </summary>
    public string ReadAllText(Encoding encoding) => File.ReadAllText(Value, encoding);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllTextAsync(string,CancellationToken)"/>
    /// </summary>
    public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) => File.ReadAllTextAsync(Value, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllTextAsync(string,Encoding,CancellationToken)"/>
    /// </summary>
    public Task<string> ReadAllTextAsync(Encoding encoding, CancellationToken cancellationToken = default) => File.ReadAllTextAsync(Value, encoding, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllBytes"/>
    /// </summary>
    public byte[] ReadAllBytes() => File.ReadAllBytes(Value);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllBytesAsync"/>
    /// </summary>
    public Task<byte[]> ReadAllBytesAsync() => File.ReadAllBytesAsync(Value);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllLines(string)"/>
    /// </summary>
    public string[] ReadAllLines() => File.ReadAllLines(Value);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllLines(string,Encoding)"/>
    /// </summary>
    public string[] ReadAllLines(Encoding encoding) => File.ReadAllLines(Value, encoding);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllLinesAsync(string,CancellationToken)"/>
    /// </summary>
    public Task<string[]> ReadAllLinesAsync(CancellationToken cancellationToken = default) => File.ReadAllLinesAsync(Value, cancellationToken);
    /// <summary>
    /// <inheritdoc cref="File.ReadAllLinesAsync(string,Encoding,CancellationToken)"/>
    /// </summary>
    public Task<string[]> ReadAllLinesAsync(Encoding encoding, CancellationToken cancellationToken = default) => File.ReadAllLinesAsync(Value, encoding, cancellationToken);
    #endregion

    /// <summary>
    /// creates the directory of this file directory
    /// </summary>
    public void CreateDirectory() => GetDirectory()?.Create();
    #endregion

    #region fileSystem operations
    /// <summary>
    /// <inheritdoc cref="File.Copy(string,string)"/>
    /// </summary>
    public void Copy(AbsoluteFilePath destination) => File.Copy(Value, destination.Value);
    /// <summary>
    /// <inheritdoc cref="File.Copy(string,string,bool)"/>
    /// </summary>
    public void Copy(AbsoluteFilePath destination, bool overwrite) => File.Copy(Value, destination.Value, overwrite);
    /// <summary>
    /// <inheritdoc cref="File.Move(string,string)"/>
    /// </summary>
    public void Move(AbsoluteFilePath destination) => File.Move(Value, destination.Value);
    /// <summary>
    /// <inheritdoc cref="File.Move(string,string,bool)"/>
    /// </summary>
    public void Move(AbsoluteFilePath destination, bool overwrite) => File.Move(Value, destination.Value, overwrite);
    /// <summary>
    /// <inheritdoc cref="File.Move(string,string)"/>
    /// </summary>
    public void Rename(FileNameWithExtension newName) => Move(GetDirectory() + newName);
    /// <summary>
    /// <inheritdoc cref="File.Move(string,string,bool)"/>
    /// </summary>
    public void Rename(FileNameWithExtension newName, bool overwrite) => Move(GetDirectory() + newName, overwrite);
    /// <summary>
    /// <inheritdoc cref="File.Delete"/>
    /// </summary>
    public void Delete() => File.Delete(Value);

    #endregion

}