using FluentAssertions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JLib.AutoMapper;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public class DataPackageGuardrailTests
{
    private readonly ITestOutputHelper _toh;

    public sealed class DuplicatePropertyDp : ValidDataPackages.Base2Dp<object>
    {
        public new DataPackageTestBase.TestTypeId Id { get; init; } = null!;

        public DuplicatePropertyDp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }


    }

    public DataPackageGuardrailTests(ITestOutputHelper toh)
    {
        _toh = toh;
    }
    private TException RunTest<TException>(Action<IServiceProvider> providerAction,params ITypePackage[] additionalTypes)
        where TException : Exception
    {
        var action = () =>
        {
            using var exceptions = new ExceptionBuilder(GetType().FullName());
            var logger = LoggerFactory.Create(x => x.AddXunit(_toh));
            var services = new ServiceCollection()
                .AddTypeCache(out var typeCache, exceptions, logger,
                    additionalTypes.Append(JLibDataGenerationTp.Instance).ToArray())
                .AddAutoMapper(x => x.AddProfiles(typeCache, logger))
                .AddLogging(x => x.AddXunit(_toh))
                .AddDataPackages(typeCache, new() { DefaultNamespace = "JLib.DataGeneration.Tests" });
            using var provider = services.BuildServiceProvider();
            providerAction(provider);
            exceptions.ThrowIfNotEmpty();
        };
        return action.Should().Throw<TException>().And;
    }

    [Fact]
    public void DuplicateIdDeclaration()
    {

        RunTest<AggregateException>(
            p=>p.IncludeDataPackages<DuplicatePropertyDp>(),
            TypePackage.Get(typeof(DuplicatePropertyDp)));
    }
}