using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JLib.DataProvider.Testing;

public record ReflectionTestOptions(string TestName, string[] ExpectedBehavior, Type[] IncludedTypes,
    Action<IServiceCollection, ITypeCache, ILoggerFactory, ExceptionBuilder> ServiceFactory, bool TestException = true,
    bool TestCache = true,
    bool TestServices = true)
{
    public ReflectionTestOptions(string TestName, string[] ExpectedBehavior, IEnumerable<Type> IncludedTypes,
        Action<IServiceCollection, ITypeCache, ILoggerFactory, ExceptionBuilder> ServiceFactory, bool TestException = true,
        bool TestCache = true,
        bool TestServices = true) :
        this(TestName, ExpectedBehavior, IncludedTypes.ToArray(), ServiceFactory, TestException, TestCache, TestServices)
    { }
}