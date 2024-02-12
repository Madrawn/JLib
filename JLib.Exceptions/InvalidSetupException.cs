namespace JLib.Exceptions;

/// <summary>
/// Indicates, that the setup of the application is invalid. This could be caused a failed validation of types.
/// </summary>
public class InvalidSetupException : JLibException
{
    public InvalidSetupException(string message) : base(message)
    {
    }

    public InvalidSetupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}