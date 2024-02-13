namespace JLib.Helper;

public static class StringHelper
{
    public static bool IsNullOrWhitespace(this string? value)
        => string.IsNullOrWhiteSpace(value);

    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrEmpty(value);
}