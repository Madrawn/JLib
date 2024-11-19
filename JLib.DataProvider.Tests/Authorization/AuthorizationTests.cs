using FluentAssertions;
using JLib.Cqrs;
using JLib.DataGeneration;
using JLib.DataProvider.Authorization;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.Tests.Authorization;

public abstract class AuthorizationTestsBaseTypes
{

    #region test classes
    public class TestDataObject : ICommandEntity
    {
        public Guid Id { get; init; }
        public bool IsAuthorized { get; set; }
        public string Name { get; set; } = "";
    }

    public record TestDataObjectId(Guid Value) : GuidValueType(Value)
    {
        public static implicit operator TestDataObjectId(Guid id) => new(id);
        public static implicit operator Guid(TestDataObjectId id) => id.Value;
    }
    public class TestObjectDataPackage : DataPackage
    {
        public TestDataObjectId FirstAuthorizedId { get; init; } = null!;
        public TestDataObjectId SecondAuthorizedId { get; init; } = null!;
        public TestDataObjectId ThirdAuthorizedId { get; init; } = null!;
        public TestDataObjectId FirstUnauthorizedId { get; init; } = null!;
        public TestDataObjectId SecondUnAuthorizedId { get; init; } = null!;
        public TestDataObjectId ThirdUnAuthorizedId { get; init; } = null!;
        public TestObjectDataPackage(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serviceProvider.GetRequiredServices(out IDataProviderRw<TestDataObject> dataProvider);
            dataProvider.Add(new TestDataObject[]
            {
                new()
                {
                    Id= FirstAuthorizedId,
                    IsAuthorized = true,
                    Name = this.GetInfoText(x => x.FirstAuthorizedId)
                },
                new()
                {
                    Id= SecondAuthorizedId,
                    IsAuthorized = true,
                    Name = this.GetInfoText(x => x.SecondAuthorizedId)
                },
                new()
                {
                    Id= ThirdAuthorizedId,
                    IsAuthorized = true,
                    Name = this.GetInfoText(x => x.ThirdAuthorizedId)
                },
                new()
                {
                    Id= FirstUnauthorizedId,
                    IsAuthorized = false,
                    Name = this.GetInfoText(x => x.FirstUnauthorizedId)
                },
                new()
                {
                    Id= SecondUnAuthorizedId,
                    IsAuthorized = false,
                    Name = this.GetInfoText(x => x.SecondUnAuthorizedId)
                },
                new()
                {
                    Id= ThirdUnAuthorizedId,
                    IsAuthorized = false,
                    Name = this.GetInfoText(x => x.ThirdUnAuthorizedId)
                },
            });
        }
    }

    public class TestAuthorizationCondition
    {
        public bool AuthorizationEnabled;
    }
    #endregion
}
public abstract class AuthorizationTestsBase<TProfile> : AuthorizationTestsBaseTypes
    where TProfile : AuthorizationProfile
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataProviderRw<TestDataObject> _dataProvider;
    private readonly TestAuthorizationCondition _authEnabler;
    private readonly TestObjectDataPackage _dataPackage;

    protected AuthorizationTestsBase(ITestOutputHelper testOutputHelper)
    {

        using var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var exceptions = new ExceptionBuilder("test");
        var services = new ServiceCollection()
            .AddLogging()
            .AddTypeCache(
                out var typeCache, exceptions, loggerFactory,
                JLibDataProviderTp.Instance,
                JLibDataGenerationTp.Instance,
                JLibCqrsTp.Instance,
                TypePackage.GetNested<AuthorizationTestsBaseTypes>(),
                TypePackage.GetNested(GetType())
                    )
            .AddSingleton<TestAuthorizationCondition>()
            .AddDataPackages(typeCache)
            .AddScopeProvider()
            .AddDataAuthorization(typeCache)
            .AddAutoMapper(b => { b.CreateMap<Guid, Guid>(); })
            .AddDataProvider<CommandEntityType, InMemoryDataProvider<IEntity>, IEntity>(
                typeCache, null, null, null, exceptions, loggerFactory)
            ;
        _serviceProvider = services.BuildServiceProvider();
        _serviceProvider.IncludeDataPackages<TestObjectDataPackage>();
        _dataProvider = _serviceProvider.GetRequiredService<IDataProviderRw<TestDataObject>>();
        _authEnabler = _serviceProvider.GetRequiredService<TestAuthorizationCondition>();
        _authEnabler.AuthorizationEnabled = true;
        _dataPackage = _serviceProvider.GetRequiredService<TestObjectDataPackage>();
    }
    #region tests
    [Fact]
    public void SetupWorks()
    {
        _authEnabler.AuthorizationEnabled = false;
        var p = _serviceProvider.GetRequiredService<IDataProviderRw<TestDataObject>>();
        p.Get().Should().HaveCount(6);
        p.Get().Select(x => x.Id).Should().NotContain(Guid.Empty);
        _authEnabler.AuthorizationEnabled = true;
    }
    [Fact]
    public void GetWorks()
    {
        _dataProvider.Get().Should().HaveCount(3);
    }
    [Fact]
    public void GetByIdWorks()
    {
        _dataProvider.Get(_dataPackage.FirstAuthorizedId)
            .Id
            .Should()
            .Be(_dataPackage.FirstAuthorizedId);
    }
    [Fact]
    public void GetByIdUnauthorizedWorks()
    {
        var f = () =>
        {
            _dataProvider.Get(_dataPackage.FirstUnauthorizedId)
                .Id
                .Should()
                .Be(_dataPackage.FirstUnauthorizedId);
        };
        f.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetByIdsUnauthorizedWorks()
    {
        var id1 = _dataPackage.FirstAuthorizedId.Value;
        var idU1 = _dataPackage.FirstUnauthorizedId.Value;
        var idU2 = _dataPackage.SecondAuthorizedId.Value;
        var f = () =>
        {
            _dataProvider.Get(new[] { id1, idU1, idU2 });
        };
        f.Should().Throw<AggregateException>();
    }
    [Fact]
    public void ContainsUnauthorized()
    {
        var id = _dataPackage.FirstUnauthorizedId.Value;
        _dataProvider.Contains(id).Should().BeFalse();
    }
    [Fact]
    public void ContainsAuthorized()
    {
        var id = _dataPackage.FirstAuthorizedId.Value;
        _dataProvider.Contains(id).Should().BeTrue();
    }
    #endregion
}

public class AuthorizationTests : AuthorizationTestsBase<AuthorizationTests.TestAuthProfile>
{
    public AuthorizationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
    public class TestAuthProfile : AuthorizationProfile
    {
        public TestAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            AddAuthorization<TestDataObject, TestAuthorizationCondition>(
                srv => e => !srv.AuthorizationEnabled || e.IsAuthorized,
                (srv, e) => !srv.AuthorizationEnabled || e.IsAuthorized);
        }
    }
}
public class OtherAuthorizationTests : AuthorizationTestsBase<AuthorizationTests.TestAuthProfile>
{
    public OtherAuthorizationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
    public class TestAuthProfile : AuthorizationProfile
    {
        public TestAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            AddGenericAuthorization<TestDataObject>(
                (srv, e) => !srv.GetRequiredService<TestAuthorizationCondition>().AuthorizationEnabled || e.IsAuthorized,
                typeof(TestDataObject));
        }
    }
}