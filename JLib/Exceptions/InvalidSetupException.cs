namespace JLib.Exceptions;

public class InvalidSetupException : JLibException
{
    public InvalidSetupException(string message) : base(message) { }
    public InvalidSetupException(string message, Exception innerException) : base(message, innerException) { }
}