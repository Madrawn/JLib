using System.Reflection;
using JLib.Configuration;
using JLib.Exceptions;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace JLib.Helper;
public static class ConfigurationHelper
{
    /// <summary>
    /// returns a new instance of the section <typeparamref name="T"/> under the given <paramref name="configSectionName"/>
    /// <br/>supports environments using the following behavior:
    /// <example>
    /// Behavior:
    /// <code>
    /// {
    ///     Environment: "Dev1",
    ///     SectionA:{
    ///         Environment: "Dev2",
    ///         Dev1:{
    ///             MyValue:"Ignored"
    ///         },
    ///         Dev2:{
    ///             MyValue:"Used"
    ///         },
    ///         MyValue:"Ignored"
    ///     },
    ///     SectionB:{
    ///         Dev1:{
    ///             MyValue:"Used"
    ///         },
    ///         Dev2:{
    ///             MyValue:"Ignored"
    ///         },
    ///         MyValue:"Ignored"
    ///     },
    ///     SectionC:{
    ///         Environment: "",
    ///         Dev1:{
    ///             MyValue:"Ignored"
    ///         },
    ///         Dev2:{
    ///             MyValue:"Ignored"
    ///         },
    ///         MyValue:"Used"
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>

    public static T GetSection<T>(this IConfiguration config, string configSectionName)
    where T : class, new()
    {
        // code duplicated in ServiceCollectionHelper.AddAllConfigSections
        var topLevelEnvironment = config[ConfigurationSections.Environment];
        if (topLevelEnvironment != null)
            Log.Information("Loading config for top level environment {environment}", topLevelEnvironment);

        var sectionInstance = config.GetSection(configSectionName);

        var sectionEnvironment = sectionInstance[ConfigurationSections.Environment];

        var environment = sectionEnvironment ?? topLevelEnvironment;
        if (environment is not null)
        {
            var environmentType = sectionEnvironment is not null ? "override" : "topLevel";
            Log.Information("Loading section {environment}.{section} ({sectionType}). Environment is defined in {environmentType}",
                environment, configSectionName, typeof(T).FullClassName(true), environmentType);
            sectionInstance = sectionInstance.GetSection(environment);
        }
        else
            Log.Information("Loading section {section} ({sectionType})", configSectionName, typeof(T).FullClassName(true));

        var obj = new T();
        sectionInstance.Bind(obj);
        return obj;
    }

    public static T GetSection<T>(this IConfiguration config) 
        where T : class, new()
    {
        var sectionName = typeof(T).GetCustomAttribute<ConfigSectionNameAttribute>()?.SectionName
            ?? throw new InvalidSetupException(
                $"missing {nameof(ConfigSectionNameAttribute)} on class {typeof(T).FullClassName()}");
        return config.GetSection<T>(sectionName.Value);
    }
}
