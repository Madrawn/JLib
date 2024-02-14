namespace JLib.Helper;

public static class ExceptionHelper
{

    /// <summary>
    /// throws the <paramref name="exception"/> if it is not null
    /// </summary>
    /// <param name="exception">the <see cref="Exception"/> to be thrown</param>
    public static void Throw(this Exception? exception)
    {
        if (exception is not null)
            throw exception;
    }
}