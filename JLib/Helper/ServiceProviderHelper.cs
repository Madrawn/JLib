namespace JLib.Helper;


public static class ServiceProviderHelper
{
    public static TSrvContainer GetRequiredServices<TSrvContainer>(this IServiceProvider provider)
        where TSrvContainer : ServiceContainer, new()
    {
        var c = new TSrvContainer();
        c.Init(provider);
        return c;
    }
}
