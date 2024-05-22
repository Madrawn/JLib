# Project. JLib.Configuration
provides reflection based config section access either direct or via dependency injection.

## Adding Config to Service Collections

```cs
using JLib.Configuration;
using JLib.Reflection.DependencyInjection;

ICOnfiguration myConfig = ...;

// ...
var services = new ServiceCollection ()
    .AddTypeCache(out var typeCache, nameof(JLib))
    .AddAllConfigSections(myConfig, typeCache)
    // ...
    ;
// ...

using var provider = services.BuildServiceProvider();

var section = provider.Services.GetRequiredService<MyConfigSection>();

[ConfigSectionName("Test")]
public class MyConfigSection
{
    public string MyValue { get; init; }
}
```

## Directly retrieving config section objects

```cs
using JLib.Configuration;

IConfiguration myConfig = ...;

var configSection myConfig.GetConfigSectionObject<MyConfigSection>();

[ConfigSectionName("Test")]
public class MyConfigSection
{
    public string MyValue { get; init; }
}
```