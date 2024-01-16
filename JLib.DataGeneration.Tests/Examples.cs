using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
// <PackageReference Include="FluentAssertions" Version="6.12.0" />
using FluentAssertions;
using System.Runtime.CompilerServices;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
// <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="7.0.14" />
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
// <PackageReference Include="Snapshooter.Xunit" Version="0.14.0" />
using Snapshooter.Xunit;
using Snapshooter;
// <PackageReference Include="xunit" Version="2.4.2" />
// <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5">
//   <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
//   <PrivateAssets>all</PrivateAssets>
// </PackageReference>
// <PackageReference Include="coverlet.collector" Version="3.2.0">
//   <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
//   <PrivateAssets>all</PrivateAssets>
// </PackageReference>
using Xunit;

namespace JLib.DataGeneration.Tests;

public class ExampleUnitTests : IDisposable
{
    [Fact]
    public async Task ImportLocationsOnly() => await RunTest(new[] { typeof(DefaultArticleDp) },
        async () =>
        {
            foreach (var article in _dbContext.Articles)
                article.Name = "updated " + article.Name;
            await _dbContext.SaveChangesAsync(_cancellationToken);
        });


    // pragmas are used to allow for (id) property declaration without explicitly setting them to null and using said values to create entities.
    // the ids are said in the ctor of the base class
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // DataPackage
    public class DefaultArticleDp : DataPackage
    {
        public ArticleId ArticleId { get; init; }
        public DefaultArticleDp(IDataPackageManager packageManager, ShopDbContext dbContext) : base(packageManager)
        {
            dbContext.Articles.Add(new()
            {
                Id = ArticleId.Value,
                Name = GetInfoText(nameof(ArticleId))
            });
        }
    }
#pragma warning restore CS8602
#pragma warning restore CS8618

    private readonly CancellationToken _cancellationToken;
    private readonly IServiceProvider _provider;
    private readonly ShopDbContext _dbContext;
    private readonly List<IDisposable> _disposables = new();
    private readonly ITypeCache _typeCache;
    private readonly IIdRegistry _idRegistry;

    public ExampleUnitTests()
    {
        var id = Guid.NewGuid();
        var exceptions = new ExceptionManager("test setup");
        var services = new ServiceCollection()
            .AddTypeCache(out _typeCache, exceptions,
                // defines the types which can be found by reflection. JLibDataGenerationTypePackage.Instance is required for dataPackages to work.
                TypePackage.GetNested<ExampleUnitTests>(), JLibDataGenerationTypePackage.Instance)
            .AddAutoMapper(p => p.AddProfiles(_typeCache))
            .AddDataPackages(_typeCache)
            .AddDbContext<ShopDbContext>(b => b.UseInMemoryDatabase(id.ToString()))
            ;

        var provider = services.BuildServiceProvider();
        _provider = provider;
        _disposables.Add(provider);
        var tokenSrc = Debugger.IsAttached // disable timeout when debugging
            ? new()
            : new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _disposables.Add(tokenSrc);
        _cancellationToken = tokenSrc.Token;

        _dbContext = _provider.GetRequiredService<ShopDbContext>();
        _dbContext.Database.EnsureCreated();
        _idRegistry = provider.GetRequiredService<IIdRegistry>();
        exceptions.ThrowIfNotEmpty();
    }
    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        foreach (var disposable in _disposables)
            disposable.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task RunTest(Type[] dataPackages, Func<Task> appliedActions, [CallerMemberName] string testName = "")
    {
        try
        {

            _provider.IncludeDataPackages(dataPackages.Select(dp => _typeCache.Get<DataPackageType>(dp)).ToArray());
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }
        catch (Exception e)
        {
            throw new("setup failed", e);
        }
        Exception? ex = null;

        CheckSnapshot(new("setup"), ex);

        try
        {
            await appliedActions();
        }
        catch (TaskCanceledException e)
        {
            e.Should().BeNull("the call should not timeout");
        }
        catch (Exception e)
        {
            ex = e;
        }

        CheckSnapshot(new("result"), ex);
    }

    private void CheckSnapshot(string checkType, Exception? exception, [CallerMemberName] string testName = "")
    {
        new Dictionary<string, object?>
        {
            {
                "Database",
                _dbContext.Articles
                    .Select(article =>
                    new {
                        article.Id,
                        IdInfo = article.Id.IdInfo(_idRegistry),
                        article.Name
                    })
            },
            {
                "exception",
                exception?.Message
            }
        }.MatchSnapshot(new SnapshotNameExtension(testName, checkType),
            o => o
                // should be ignored since the value might change if tests are run in parallel.
                .IgnoreField("$.Database[*].IdInfo")

        );
    }



    #region referenced types
    /// <summary>
    /// id ValueType
    /// </summary>
    public record ArticleId(Guid Value) : GuidValueType(Value);

    /// <summary>
    /// DbEntity
    /// </summary>
    public class Article
    {
        [Key]
        public Guid Id { get; init; }

        public string Name { get; set; } = "";
    }

    /// <summary>
    /// DbContext
    /// </summary>
    public class ShopDbContext : DbContext
    {
        public virtual DbSet<Article> Articles { get; init; } = null!;

        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {

        }
    }

    #endregion


}