namespace JLib.Exceptions;

public class InvalidConfigException : JLibException
{
    public InvalidConfigException(string message) : base(message) { }
}