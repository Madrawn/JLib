namespace JLib.Helper;

public static class CharHelper
{
    /// <summary>
    /// <inheritdoc cref="char.IsDigit(char)"/>
    /// </summary>
    public static bool IsDigit(this char c)
        => char.IsDigit(c);
    /// <summary>
    /// <inheritdoc cref="char.IsLetter(char)"/>
    /// </summary>
    public static bool IsLetter(this char c)
        => char.IsLetter(c);
    /// <summary>
    /// <inheritdoc cref="char.IsLetterOrDigit(char)"/>
    /// </summary>
    public static bool IsLetterOrDigit(this char c)
        => char.IsLetterOrDigit(c);
}