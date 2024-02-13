using JLib.Cqrs;
using JLib.DataProvider.Testing;
using JLib.Exceptions;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.Tests.AddDataProvider;

public class AddDataProviderTests : ReflectionTestBase
{


    #region Repository: None       DataProvider: ReadOnly           NoRepo_ProvR
    [Fact]
    public void NoRepo_ProvR()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Read Only",
                "  Repository           : -",
                "Expected Implementations:",
                "  IDataProviderR       : DataProvider",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: -",
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderR<>),
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                    cache, null, null, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: ReadWrite          NoRepo_ProvRw
    [Fact]
    public void NoRepo_ProvRw()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider         : Read Write",
                "  Repository           : -",
                "Expected Implementations:",
                "  IDataProviderR       : DataProvider",
                "  IDataProviderRw      : DataProvider",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: DataProvider",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                cache, null, null, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: Forced ReadOnly    NoRepo_ProvFr
    [Fact]
    public void NoRepo_ProvFr()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider         : Forced Read Only",
                "  Repository           : -",
                "Expected Implementations:",
                "  IDataProviderR       : DataProvider",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                cache, null, _ => true, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: ReadOnly   DataProvider: ReadOnly           RepoR_ProvR
    [Fact]
    public void RepoR_ProvR()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider         : Read Only",
                "  Repository           : Read Only",
                "Expected Implementations:",
                "  IDataProviderR       : Repository",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestDataProviderR<>),
                typeof(TestRepositoryR)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                cache, null, null, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: ReadOnly           NoRepo_ProvR_Filter
    [Fact]
    public void NoRepo_ProvR_Filter()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider         : Read Only",
                "  Repository           : -",
                "  Filter               : only Ce2",
                "Expected Implementations:",
                $"  {nameof(TestCommandEntity)}",
                "  IDataProviderR       : -",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : -",
                "  ISourceDataProviderRw: -",
               $"  {nameof(TestCommandEntity2)}",
                "  IDataProviderR       : DataProvider",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestCommandEntity2),
                typeof(TestDataProviderR<>)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                cache, tvt => tvt.Value == typeof(TestCommandEntity2), null, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: ReadOnly           NoRepo_ProvR_FilteredInv
    [Fact]
    public void NoRepo_ProvR_FilteredInv()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider         : Read Only",
                "  Repository           : -",
                "  Filter               : only Ce2",
                "Expected Implementations:",
                $"  {nameof(TestCommandEntity)}",
                "  IDataProviderR       : -",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : -",
                "  ISourceDataProviderRw: -",
                $"  {nameof(TestCommandEntity2)}",
                "  IDataProviderR       : DataProvider",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestCommandEntity2),
                typeof(TestRepositoryRw),
                typeof(TestDataProviderR<>)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                cache, tvt => tvt.Value == typeof(TestCommandEntity2), null, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: ReadOnly           NoRepo_ProvR_2Ce
    [Fact]
    public void NoRepo_ProvR_2Ce()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider           : Read Only",
                "  Repository             : -",
                "Expected Implementations:",
               $"  {nameof(TestCommandEntity)}",
                "    IDataProviderR       : DataProvider",
                "    IDataProviderRw      : -",
                "    ISourceDataProviderR : DataProvider",
                "    ISourceDataProviderRw: -",
               $"  {nameof(TestCommandEntity2)}",
                "    IDataProviderR       : DataProvider",
                "    IDataProviderRw      : -",
                "    ISourceDataProviderR : DataProvider",
                "    ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestCommandEntity2),
                typeof(TestDataProviderR<>)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                cache, null, null, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: ReadOnly           NoRepo_Prov`2_Ce2
    [Fact]
    public void NoRepo_Prov_2_Ce2()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider           : Read Only",
                "  Repository             : -",
                "Expected Implementations:",
                $"  {nameof(TestCommandEntity)}",
                "    IDataProviderR       : -",
                "    IDataProviderRw      : -",
                "    ISourceDataProviderR : -",
                "    ISourceDataProviderRw: -",
                $"  {nameof(TestCommandEntity2)}",
                "    DataProvider         : DataProvider<Ce,Ce2>",
                "    IDataProviderR       : DataProvider",
                "    IDataProviderRw      : -",
                "    ISourceDataProviderR : DataProvider",
                "    ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestCommandEntity2),
                typeof(TestDataProviderR<,>)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderR<ITestEntity1, ITestEntity2>, ITestEntity2>(
                cache, ce => ce.Value == typeof(TestCommandEntity), null,
                new Func<CommandEntityType, ITypeValueType>[]
                {
                        ce=>ce,
                        _=>cache.Get<CommandEntityType>(typeof(TestCommandEntity2)),
                },
                exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: None       DataProvider: Ce R, Ce2 Rw       NoRepo_ProvCeR_provCe2Rw
    [Fact]
    public void NoRepo_ProvR_SelRo()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider           : Read Only",
                "  Repository             : -",
                "  ReadOnlySelect         : Ce R, Ce2 Rw",
                "Expected Implementations:",
               $"  {nameof(TestCommandEntity)}",
                "    IDataProviderR       : DataProvider",
                "    IDataProviderRw      : -",
                "    ISourceDataProviderR : DataProvider",
                "    ISourceDataProviderRw: -",
               $"  {nameof(TestCommandEntity2)}",
                "    IDataProviderR       : DataProvider",
                "    IDataProviderRw      : DataProvider",
                "    ISourceDataProviderR : DataProvider",
                "    ISourceDataProviderRw: DataProvider",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestCommandEntity2),
                typeof(TestDataProviderRw<>)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                cache, null, tvt => tvt.Value == typeof(TestCommandEntity), null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: ReadOnly   DataProvider: ReadWrite          RepoR_ProvRw

    [Fact]
    public void RepoR_ProvRw()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Read Write",
                "  Repository           : Read Only",
                "Expected Error:",
                "  An error which tells you to either force ReadOnly on the DataProvider or implement IDataProviderRw on the Repository",
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
                typeof(TestRepositoryR)
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                    cache, null, null, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: ReadOnly   DataProvider: Forced ReadOnly    RepoR_ProvFr
    [Fact]
    public void RepoR_ProvFr()
        => base.Test(
            new[]{
                "Provided:",
                "  DataProvider         : Forced Read Only",
                "  Repository           : ReadOnly",
                "Expected Implementations:",
                "  IDataProviderR       : Repository",
                "  IDataProviderRw      : -",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: -",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
                typeof(TestRepositoryR)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                cache, null, _ => true, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: ReadWrite  DataProvider: ReadOnly           RepoRw_ProvR

    [Fact]
    public void RepoRw_ProvR()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Read Only",
                "  Repository           : Read Write",
                "Expected Exception:",
                "  an exception, which tells the user to provide the data provider as Read Write or not implement IDataProviderRw"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderR<>),
                typeof(TestRepositoryRw)
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                    cache, null, null, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: ReadWrite  DataProvider: ReadWrite          RepoRw_ProvRw
    [Fact]
    public void RepoRw_ProvRw()
        => base.Test(
            new[]
        {
                "Provided:",
                "  DataProvider         : Read Write",
                "  Repository           : Read Write",
                "Expected Implementations:",
                "  IDataProviderR       : Repository",
                "  IDataProviderRw      : Repository",
                "  ISourceDataProviderR : DataProvider",
                "  ISourceDataProviderRw: DataProvider",
        },
        new[]
        {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
                typeof(TestRepositoryRw)
        },
        (services, cache, loggerFactory, exceptions) =>
            services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                cache, null, null, null, exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)), loggerFactory)
        );
    #endregion
    #region Repository: ReadWrite  DataProvider: Forced ReadOnly    RepoRw_ProvFr

    [Fact]
    public void RepoRw_ProvFr()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Forced ReadOnly",
                "  Repository           : Read Write",
                "Expected Exception:",
                "  An exception which tells the user to not provide the DataProvider as ReadOnly and informs that it is forced"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
                typeof(TestRepositoryRw)
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                    cache, null, _ => true, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: InvalidRw  DataProvider: ReadWrite          RepoIrw_ProvRw

    [Fact]
    public void RepoIrw_ProvRw()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Read Write",
                "  Repository           : Invalid ReadWrite",
                "Expected Error:",
                "  An error which tells you that repositories must not implement the ISourceDataProvider Interface and that you must only implement the IDataProvider Interfaces"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
                typeof(TestInvalidRepositoryRw)
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                    cache, null, null, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: InvalidR   DataProvider: ReadOnly           RepoIr_ProvR

    [Fact]
    public void RepoIr_ProvR()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : ReadOnly",
                "  Repository           : Invalid ReadOnly",
                "Expected Exception:",
                "  An exception which tells the user to not provide the DataProvider as ReadOnly and informs that it is forced"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderR<>),
                typeof(TestInvalidRepositoryR)
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>, IEntity>(
                    cache, null, _ => true, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: None       DataProvider: InvalidR           NoRepo_ProvIr

    [Fact]
    public void NoRepo_ProvIr()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Invalid ReadOnly",
                "  Repository           : -",
                "Expected Exception:",
                "  An exception which tells the user to implement the ISourceDataProviderR Interface"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestInvalidDataProviderR<>),
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestInvalidDataProviderR<IEntity>, IEntity>(
                    cache, null, _ => true, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: None       DataProvider: InvalidRw          NoRepo_ProvIrw

    [Fact]
    public void NoRepo_ProvIrw()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Invalid ReadWrite",
                "  Repository           : -",
                "Expected Exception:",
                "  An exception which tells the user to implement the SourceDataProvider Interfaces"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestInvalidDataProviderRw<>),
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestInvalidDataProviderRw<IEntity>, IEntity>(
                    cache, null, _ => true, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: Multiple       DataProvider: InvalidRw      MultiRepo_ProvIrw

    [Fact]
    public void MultiRepo_ProvIrw()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : ReadWrite",
                "  Repository           : R, Rw",
                "Expected Error:",
                "  An error which tells the user that multiple repositories for the same dataObject have been found"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestDataProviderRw<>),
                typeof(TestRepositoryR),
                typeof(TestRepositoryRw)
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                    cache, null, null, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion
    #region Repository: None       DataProvider: Invalid Interfaces NoRepo_ProvInvInt

    [Fact]
    public void NoRepo_ProvInvInt()
        => base.Test(
            new[]
            {
                "Provided:",
                "  DataProvider         : Invalid Interfaces",
                "  Repository           : None",
                "Expected Error:",
                "  An error which tells the user that this combination of interfaces is not supported"
            },
            new[]
            {
                typeof(TestCommandEntity),
                typeof(TestInvalidInterfaceDataProvider<>),
            },
            (services, cache, loggerFactory, exceptions) =>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>, IEntity>(
                    cache, null, _ => true, null,
                    exceptions.CreateChild(nameof(DataProviderServiceCollectionExtensions.AddDataProvider)),
                    loggerFactory),
            true, false, false
        );
    #endregion


















    public AddDataProviderTests(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    protected override void AddServices(IServiceCollection services, ITypeCache cache, IExceptionManager exceptions)
        => services.AddRepositories(cache, exceptions);



    #region test classes
    #region entities
    public class TestCommandEntity : ITestEntity1
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
    public class TestCommandEntity2 : ITestEntity2
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
    public interface ITestEntity1 : ICommandEntity { }
    public interface ITestEntity2 : ICommandEntity { }
    #endregion
    #region repositories

    public class TestInvalidDataProviderR<T> : DataProviderRBase<T>, IDataProviderR<T> where T : IEntity
    {
        public override IQueryable<T> Get() => throw new NotImplementedException();
    }
    public class TestInvalidDataProviderRw<T> : DataProviderRBase<T>, IDataProviderRw<T> where T : IEntity
    {
        public override IQueryable<T> Get() => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();

        public void Add(IReadOnlyCollection<T> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();
        public void Remove(T item) => throw new NotImplementedException();

        public void Remove(IReadOnlyCollection<Guid> itemIds) => throw new NotImplementedException();
        public void Remove(IReadOnlyCollection<T> items) => throw new NotImplementedException();
    }

    public class TestRepositoryR : DataProviderRBase<TestCommandEntity>, IDataProviderR<TestCommandEntity>
    {
        public TestRepositoryR(ISourceDataProviderR<TestCommandEntity> _)
        {

        }
        public override IQueryable<TestCommandEntity> Get()
            => throw new NotImplementedException();
    }
    public class TestRepositoryRw : DataProviderRBase<TestCommandEntity>, IDataProviderRw<TestCommandEntity>
    {
        public TestRepositoryRw()
        {

        }
        public override IQueryable<TestCommandEntity> Get()
            => throw new NotImplementedException();

        public void Add(TestCommandEntity item) => throw new NotImplementedException();

        public void Add(IReadOnlyCollection<TestCommandEntity> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();
        public void Remove(TestCommandEntity item) => throw new NotImplementedException();

        public void Remove(IReadOnlyCollection<Guid> itemIds) => throw new NotImplementedException();
        public void Remove(IReadOnlyCollection<TestCommandEntity> items) => throw new NotImplementedException();
    }

    public class TestInvalidRepositoryR : DataProviderRBase<TestCommandEntity>, ISourceDataProviderR<TestCommandEntity>
    {
        public override IQueryable<TestCommandEntity> Get() => throw new NotImplementedException();
    }
    public class TestInvalidRepositoryRw : DataProviderRBase<TestCommandEntity>, ISourceDataProviderRw<TestCommandEntity>
    {
        public override IQueryable<TestCommandEntity> Get() => throw new NotImplementedException();

        public void Add(TestCommandEntity item) => throw new NotImplementedException();

        public void Add(IReadOnlyCollection<TestCommandEntity> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();
        public void Remove(TestCommandEntity item) => throw new NotImplementedException();

        public void Remove(IReadOnlyCollection<Guid> itemIds) => throw new NotImplementedException();
        public void Remove(IReadOnlyCollection<TestCommandEntity> items) => throw new NotImplementedException();
    }
    #endregion
    #region DataProvider
    public class TestDataProviderRw<T> : DataProviderRBase<T>, ISourceDataProviderRw<T> where T : IEntity
    {
        public override IQueryable<T> Get() => throw new NotImplementedException();

        public void Add(T item) => throw new NotImplementedException();

        public void Add(IReadOnlyCollection<T> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();
        public void Remove(T item) => throw new NotImplementedException();

        public void Remove(IReadOnlyCollection<Guid> itemIds) => throw new NotImplementedException();
        public void Remove(IReadOnlyCollection<T> items) => throw new NotImplementedException();
    }
    public class TestDataProviderR<T1, T2> : DataProviderRBase<T2>, ISourceDataProviderR<T2>
        where T1 : ITestEntity1
        where T2 : ITestEntity2
    {
        public override IQueryable<T2> Get() => throw new NotImplementedException();
    }
    public class TestDataProviderR<T> : DataProviderRBase<T>, ISourceDataProviderR<T> where T : IEntity
    {
        public override IQueryable<T> Get() => throw new NotImplementedException();
    }

    public class TestInvalidInterfaceDataProvider<T> : DataProviderRBase<T>, ISourceDataProviderR<T>, IDataProviderRw<T>
        where T : IEntity
    {
        public override IQueryable<T> Get() => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();

        public void Add(IReadOnlyCollection<T> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();
        public void Remove(T item) => throw new NotImplementedException();

        public void Remove(IReadOnlyCollection<Guid> itemIds) => throw new NotImplementedException();
        public void Remove(IReadOnlyCollection<T> items) => throw new NotImplementedException();
    }
    #endregion
    #endregion

}
