using JLib.Helper;

namespace JLib.Exceptions;

public class InvalidTypeException : InvalidSetupException
{
    public InvalidTypeException(Type tvt, Type value, string message) : base(
        message)
    {
    }
}