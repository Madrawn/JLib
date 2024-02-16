namespace JLib.Helper;
public static class DisposableHelper
{
    public static T DisposeWith<T>(this T disposable, IList<IDisposable> disposables)
        where T:IDisposable
    {
        disposables.Add(disposable);
        return disposable;
    }

    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        foreach (var disposable in disposables)
            disposable.Dispose();
    }
}
