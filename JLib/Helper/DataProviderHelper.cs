using JLib.Data;

namespace JLib.Helper;

public static class DataProviderHelper
{
    /// <summary>
    /// returns the service implementations and their DI dependencies recursively of the given <see cref="IDataProviderR{TData}"/> Type provided via <paramref name="serviceType"/>
    /// <br/>note: the dependent services are instantiated to analyze their implementation type
    /// <br/>note: the <paramref name="serviceProvider"/> has to be scoped
    /// <br/>info: the dependencies are pulled from the constructor parameters
    /// </summary>
    public static Dictionary<string, object> AnalyzeDependencies(Type serviceType, IServiceProvider serviceProvider)
    {
        var properties = new Dictionary<string, object>();
        Type? implementationType;
        try
        {
            implementationType = serviceProvider.GetService(serviceType)?.GetType();
        }
        catch (Exception e)
        {
            properties.Add("error", "data provider could not be retrieved");
            properties.Add("exception message", e.Message);
            properties.Add("exception ToString", e.ToString());
            return properties;
        }

        properties.Add("service", serviceType.FullClassName());

        if (!serviceType.ImplementsAny<IDataProviderR<IEntity>>())
        {
            properties.Add("error", "not a data provider");
            return properties;
        }

        if (implementationType is null)
        {
            properties.Add("error", "no implementation found");
            return properties;
        }


        properties.Add("implementation", implementationType.FullClassName());

        properties.Add("implemented services",
            implementationType.GetInterfaces().Select(i => i.FullClassName()).ToArray());


        var ctor = implementationType.GetConstructors().First();
        var ctorParams = ctor.GetParameters();
        properties.Add("service dependencies",
            ctorParams.ToDictionary(param => param.Name ?? "",
                param => AnalyzeDependencies(param.ParameterType, serviceProvider)));

        return properties;
    }
}