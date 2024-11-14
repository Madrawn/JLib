namespace JLib.Helper;

/// <summary>
/// contains extension methods for working with <see cref="IDisposable"/>s
/// </summary>
public static class DisposableHelper
{
    /// <summary>
    /// adds the <paramref name="disposable"/> to the <paramref name="disposables"/> list and returns the <paramref name="disposable"/>
    /// </summary>
    /// <returns>the <paramref name="disposable"/> argument</returns>
    public static T DisposeWith<T>(this T disposable, IList<IDisposable> disposables)
        where T:IDisposable
    {
        disposables.Add(disposable);
        return disposable;
    }

    /// <summary>
    /// Disposes all <paramref name="disposables"/>
    /// </summary>
    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        foreach (var disposable in disposables)
            disposable.Dispose();
    }
}
