namespace JLib.Exceptions;

public class TvtNavigationFailedException : InvalidSetupException
{
    public TvtNavigationFailedException(string message) : base(message)
    {
    }
    public TvtNavigationFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public static TvtNavigationFailedException Create<TFrom, TTo>(Type value, string message)
        where TFrom : TypeValueType
        where TTo : TypeValueType
        => new($"could not navigate from {typeof(TFrom).Name}({value.Name}) to {typeof(TTo).Name}: {message}");
}