using FluentAssertions;
using JLib.Data;
using JLib.Data.Authorization;
using JLib.DataGeneration;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Testing;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.Tests.Data.Authorization;
public class AuthorizationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataProviderRw<TestDataObject> _dataProvider;
    private readonly IDataPackageRetriever _dataPackages;
    private readonly TestAuthorizationCondition _authEnabler;

    public AuthorizationTests()
    {
        var exceptions = new ExceptionManager("test");
        var services = new ServiceCollection()
            .AddTypeCache(
                new[]
                {
                    typeof(TypeCache).Assembly,
                    typeof(DataPackage).Assembly
                }, new[]
                {
                    typeof(TestDataObject),
                    typeof(TestObjectDataPackage),
                    typeof(TestAuthProfile),
                }, out var typeCache, exceptions)
            .AddSingleton<TestAuthorizationCondition>()
            .AddScopeProvider()
            .AddDataAuthorization(typeCache)
            .AddAutoMapper(b => { b.CreateMap<Guid, Guid>(); })
            .AddDataProvider<CommandEntityType, MockDataProvider<IEntity>, IEntity>(
                typeCache, null, null, null, exceptions)
            ;
        _serviceProvider = services.BuildServiceProvider();
        _dataPackages = DataPackageManager
            .ApplyPackages(_serviceProvider, p => p.Include<TestObjectDataPackage>());
        _dataProvider = _serviceProvider.GetRequiredService<IDataProviderRw<TestDataObject>>();
        _authEnabler = _serviceProvider.GetRequiredService<TestAuthorizationCondition>();
        _authEnabler.AuthorizationEnabled = true;
    }

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
        var id = _dataPackages.GetPackage<TestObjectDataPackage>().FirstAuthorizedId;
        _dataProvider.Get(id)
            .Id
            .Should()
            .Be(id);
    }
    [Fact]
    public void GetByIdUnauthorizedWorks()
    {
        var id = _dataPackages.GetPackage<TestObjectDataPackage>().FirstUnauthorizedId;
        var f = () =>
        {
            _dataProvider.Get(id)
                .Id
                .Should()
                .Be(id);
        };
        f.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetByIdsUnauthorizedWorks()
    {
        var id1 = _dataPackages.GetPackage<TestObjectDataPackage>().FirstAuthorizedId.Value;
        var idU1 = _dataPackages.GetPackage<TestObjectDataPackage>().FirstUnauthorizedId.Value;
        var idU2 = _dataPackages.GetPackage<TestObjectDataPackage>().SecondAuthorizedId.Value;
        var f = () =>
        {
            _dataProvider.Get(new[] { id1, idU1, idU2 });
        };
        f.Should().Throw<AggregateException>();
    }
    [Fact]
    public void ContainsUnauthorized()
    {
        var id = _dataPackages.GetPackage<TestObjectDataPackage>().FirstUnauthorizedId.Value;
        _dataProvider.Contains(id).Should().BeFalse();
    }
    [Fact]
    public void ContainsAuthorized()
    {
        var id = _dataPackages.GetPackage<TestObjectDataPackage>().FirstAuthorizedId.Value;
        _dataProvider.Contains(id).Should().BeTrue();
    }
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
        public TestDataObjectId FirstAuthorizedId { get; protected set; } = null!;
        public TestDataObjectId SecondAuthorizedId { get; protected set; } = null!;
        public TestDataObjectId ThirdAuthorizedId { get; protected set; } = null!;
        public TestDataObjectId FirstUnauthorizedId { get; protected set; } = null!;
        public TestDataObjectId SecondUnAuthorizedId { get; protected set; } = null!;
        public TestDataObjectId ThirdUnAuthorizedId { get; protected set; } = null!;
        public TestObjectDataPackage(IDataPackageStore dataPackages) : base(dataPackages)
        {
            dataPackages.AddEntities(new TestDataObject[]
            {
                new()
                {
                    Id= FirstAuthorizedId,
                    IsAuthorized = true,
                    Name = GetInfoText(nameof(FirstAuthorizedId))
                },
                new()
                {
                    Id= SecondAuthorizedId,
                    IsAuthorized = true,
                    Name = GetInfoText(nameof(SecondAuthorizedId))
                },
                new()
                {
                    Id= ThirdAuthorizedId,
                    IsAuthorized = true,
                    Name = GetInfoText(nameof(ThirdAuthorizedId))
                },
                new()
                {
                    Id= FirstUnauthorizedId,
                    IsAuthorized = false,
                    Name = GetInfoText(nameof(FirstUnauthorizedId))
                },
                new()
                {
                    Id= SecondUnAuthorizedId,
                    IsAuthorized = false,
                    Name = GetInfoText(nameof(SecondUnAuthorizedId))
                },
                new()
                {
                    Id= ThirdUnAuthorizedId,
                    IsAuthorized = false,
                    Name = GetInfoText(nameof(ThirdUnAuthorizedId))
                },
            });
        }
    }

    public class TestAuthorizationCondition
    {
        public bool AuthorizationEnabled;
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
    #endregion
}
