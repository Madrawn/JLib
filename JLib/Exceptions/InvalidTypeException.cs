namespace JLib.Exceptions;
public class InvalidTypeException : InvalidSetupException
{
    public InvalidTypeException(Type tvt, Type value, string message) : base(
        $"Error while initializing {tvt.Name} with {value}: {message}")
    {

    }
}
