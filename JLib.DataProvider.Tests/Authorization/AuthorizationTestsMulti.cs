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

public class AuthorizationTestsMulti
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TestAuthorizationCondition _authEnabler;

    public AuthorizationTestsMulti(ITestOutputHelper testOutputHelper)
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
                TypePackage.GetNested<AuthorizationTestsMulti>()
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
        _serviceProvider.IncludeDataPackages<TestDataObjectDp, OtherTestDataObjectDp>();
        _authEnabler = _serviceProvider.GetRequiredService<TestAuthorizationCondition>();
        _authEnabler.AuthorizationEnabled = true;
    }


    private void Act(Action<TestObjectDpb, IDataProviderR<TestDataObjectBase>> act)
    {
        act(_serviceProvider.GetRequiredService<TestDataObjectDp>(),
            new CastDataProviderR<TestDataObject, TestDataObjectBase>(
                _serviceProvider.GetRequiredService<IDataProviderR<TestDataObject>>()));

        act(_serviceProvider.GetRequiredService<OtherTestDataObjectDp>(),
            new CastDataProviderR<OtherTestDataObject, TestDataObjectBase>(
                _serviceProvider.GetRequiredService<IDataProviderR<OtherTestDataObject>>()));
    }


    [Fact]
    public void SetupWorks()
    {
        _authEnabler.AuthorizationEnabled = false;
        Act((_, provider) =>
        {
            provider.Get().Should().HaveCount(6);
            provider.Get().Select(x => x.Id).Should().NotContain(Guid.Empty);
        });
        _authEnabler.AuthorizationEnabled = true;
    }
    [Fact]
    public void GetWorks()
        => Act((_, provider) => provider.Get().Should().HaveCount(3));

    [Fact]
    public void GetByIdWorks()
        => Act((package, provider) => provider.Get(package.FirstAuthorizedId)
            .Id
            .Should()
            .Be(package.FirstAuthorizedId));

    [Fact]
    public void GetByIdUnauthorizedWorks()
        => Act((package, provider) =>
        {
            var f = () =>
            {
                provider.Get(package.FirstUnauthorizedId)
                    .Id
                    .Should()
                    .Be(package.FirstUnauthorizedId);
            };
            f.Should().Throw<InvalidOperationException>();
        });

    [Fact]
    public void GetByIdsUnauthorizedWorks()
        => Act((package, provider) =>
        {
            var id1 = package.FirstAuthorizedId.Value;
            var idU1 = package.FirstUnauthorizedId.Value;
            var idU2 = package.SecondAuthorizedId.Value;
            var f = () =>
            {
                provider.Get(new[] { id1, idU1, idU2 });
            };
            f.Should().Throw<AggregateException>();
        });

    [Fact]
    public void ContainsUnauthorized() 
        => Act((package, provider) =>
        {
            var id = package.FirstUnauthorizedId.Value;
            provider.Contains(id).Should().BeFalse();
        });

    [Fact]
    public void ContainsAuthorized() 
        => Act((package, provider) =>
        {
            var id = package.FirstUnauthorizedId.Value;
            provider.Contains(id).Should().BeFalse();
        });

    #region test classes

    #region Entity Setup

    public abstract class TestDataObjectBase : ICommandEntity
    {
        public Guid Id { get; init; }
        public bool IsAuthorized { get; set; }
        public string Name { get; set; } = "";
    }
    public class TestDataObject : TestDataObjectBase
    {
    }

    public class OtherTestDataObject : TestDataObjectBase
    {
    }

    public record TestDataObjectId(Guid Value) : GuidValueType(Value)
    {
        public static implicit operator TestDataObjectId(Guid id) => new(id);
        public static implicit operator Guid(TestDataObjectId id) => id.Value;
    }
    public record OtherTestDataObjectId(Guid Value) : GuidValueType(Value)
    {
        public static implicit operator OtherTestDataObjectId(Guid id) => new(id);
        public static implicit operator Guid(OtherTestDataObjectId id) => id.Value;
    }
    #endregion

    #region DataPackages

    public abstract class TestObjectDpb : DataPackage
    {
        public TestDataObjectId FirstAuthorizedId { get; init; } = null!;
        public TestDataObjectId SecondAuthorizedId { get; init; } = null!;
        public TestDataObjectId ThirdAuthorizedId { get; init; } = null!;
        public TestDataObjectId FirstUnauthorizedId { get; init; } = null!;
        public TestDataObjectId SecondUnAuthorizedId { get; init; } = null!;
        public TestDataObjectId ThirdUnAuthorizedId { get; init; } = null!;
        protected TestObjectDpb(IServiceProvider serviceProvider) : base(serviceProvider) { }
    }
    public abstract class TestObjectDpb<TDo> : TestObjectDpb
    where TDo : TestDataObjectBase, new()
    {
        protected TestObjectDpb(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            serviceProvider.GetRequiredServices(out IDataProviderRw<TDo> dataProvider);
            dataProvider.Add(new TDo[]
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
    public sealed class TestDataObjectDp : TestObjectDpb<TestDataObject>
    {
        public TestDataObjectDp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
    public sealed class OtherTestDataObjectDp : TestObjectDpb<OtherTestDataObject>
    {
        public OtherTestDataObjectDp(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
    #endregion

    public class TestAuthorizationCondition
    {
        public bool AuthorizationEnabled;
    }
    public class TestAuthProfile : AuthorizationProfile
    {
        public TestAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            AddGenericAuthorization<TestDataObject, OtherTestDataObject, TestDataObjectBase>(
                (provider, e) => !provider.GetRequiredService<TestAuthorizationCondition>().AuthorizationEnabled || e.IsAuthorized);
        }
    }
    #endregion
}