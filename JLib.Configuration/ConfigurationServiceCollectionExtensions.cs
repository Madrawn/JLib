using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ITypeCache = JLib.Reflection.ITypeCache;

namespace JLib.Configuration;
/// <summary>
/// provides a  utility methods to add all config sections marked with the <see cref="ConfigSectionNameAttribute"/> to the <see cref="ServiceCollection"/> without nesting them inside a <see cref="Option{T}"/> while providing Environment Support as described in <see cref="AddAllConfigSections"/>
/// </summary>
public static class ConfigurationServiceCollectionExtensions
{
    /// <summary>
    /// adds <see cref="IOptions{TOptions}"/> for each <see cref="ConfigurationSectionType"/> and makes the value directly injectable
    /// supports multiple environments. The environment key is found under <see cref="ConfigurationSections.Environment"/>.
    /// There can be a top level environment and one per section.
    /// + <see cref="IOptions{TOptions}"/> for each <see cref="ConfigurationSectionType"/>
    /// -- a instance of each <see cref="ConfigurationSectionType"/>
    /// <br/>
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
    public static IServiceCollection AddAllConfigSections(this IServiceCollection services,
        ITypeCache typeCache, IConfiguration config, ILoggerFactory loggerFactory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var logger = loggerFactory.CreateLogger(typeof(ConfigurationServiceCollectionExtensions));
        var configMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                               .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                                   new[] { typeof(IServiceCollection), typeof(IConfiguration) })
                           ?? throw new InvalidSetupException("configure method not found");

        var topLevelEnvironment = config[ConfigurationSections.Environment];
        if (topLevelEnvironment != null)
            logger.LogInformation("Loading config for top level environment {environment}", topLevelEnvironment);
        // code duplicated in ConfigSectionHelper.GetSectionObject
        foreach (var sectionType in typeCache.All<ConfigurationSectionType>())
        {
            var sectionInstance = config.GetSection(sectionType.SectionName.Value);

            var sectionEnvironment = sectionInstance[ConfigurationSections.Environment];

            var environment = sectionEnvironment ?? topLevelEnvironment;
            if (environment is not null)
            {
                var environmentType = sectionEnvironment is not null ? "override" : "topLevel";
                logger.LogInformation(
                    "Loading section {environment}.{section} ({sectionType}). Environment is defined in {environmentType}",
                    environment, sectionType.SectionName, sectionType.Value.FullName(true), environmentType);
                sectionInstance = sectionInstance.GetSection(environment);
            }
            else
                logger.LogInformation("Loading section {section} ({sectionType})", sectionType.SectionName.Value,
                    sectionType.Value.FullName(true));

            var specificConfig = configMethod.MakeGenericMethod(sectionType.Value);

            specificConfig.Invoke(null, new object?[]
            {
                services, sectionInstance
            });

            // extract value from options and make the section directly accessible
            var src = typeof(IOptions<>).MakeGenericType(sectionType);
            var prop = src.GetProperty(nameof(IOptions<Ignored>.Value)) ??
                       throw new InvalidSetupException("Value Prop not found on options");
            services.Add(
                new ServiceDescriptor(sectionType.Value,
                    provider => prop.GetValue(provider.GetRequiredService(src))
                                ?? throw new InvalidSetupException($"options section {sectionType.Name} not found"),
                    lifetime));
        }

        return services;
    }

}
