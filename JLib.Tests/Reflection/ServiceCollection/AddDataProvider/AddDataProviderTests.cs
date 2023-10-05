using System;
using System.Collections;
using JLib.Data;
using JLib.Helper;
using JLib.Testing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit.Abstractions;
using ServiceCollectionHelper = JLib.Helper.ServiceCollectionHelper;

namespace JLib.Tests.Reflection.ServiceCollection.AddDataProvider;

public class AddDataProviderTests:ReflectionTestBase
{
    public class TestArguments : ReflectionTestArguments
    {
        protected override string Filter { get; } = "";

        protected override IEnumerable<ReflectionTestOptions> Options { get; }
            = new ReflectionTestOptions[]
        {
                    #region Repository: None       DataProvider: ReadOnly           NoRepo_ProvR
        new(
            "NoRepo_ProvR",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider)))
            ),
        #endregion
        #region Repository: None       DataProvider: ReadWrite          NoRepo_ProvRw
        new (
            "NoRepo_ProvRw",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider)))
        ),
        #endregion
        #region Repository: None       DataProvider: Forced ReadOnly    NoRepo_ProvFr
        new
        (
            "NoRepo_ProvFr",
            new[]{
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider)))
        ),
        #endregion
        #region Repository: ReadOnly   DataProvider: ReadOnly           RepoR_ProvR
        new (
            "RepoR_ProvR",
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider)))
        ),
        #endregion
        #region Repository: ReadOnly   DataProvider: ReadWrite          RepoR_ProvRw
        new
        (
            "RepoR_ProvRw",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                        cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: ReadOnly   DataProvider: Forced ReadOnly    RepoR_ProvFr
        new
        (
            "RepoR_ProvFr",
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider)))
        ),
        #endregion
        #region Repository: ReadWrite  DataProvider: ReadOnly           RepoRw_ProvR
        new (
            "RepoRw_ProvR",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: ReadWrite  DataProvider: ReadWrite          RepoRw_ProvRw
        new (
            "RepoRw_ProvRw",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider)))
        ),
        #endregion
        #region Repository: ReadWrite  DataProvider: Forced ReadOnly    RepoRw_ProvFr
        new (
            "RepoRw_ProvFr",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: InvalidRw  DataProvider: ReadWrite          RepoIrw_ProvRw
        new (
            "RepoIrw_ProvRw",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: InvalidR   DataProvider: ReadOnly               RepoIr_ProvR
        new (
            "RepoIr_ProvR",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderR<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: None       DataProvider: InvalidR               NoRepo_ProvIr
        new (
            "NoRepo_ProvIr",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestInvalidDataProviderR<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: None       DataProvider: InvalidRw                          NoRepo_ProvIrw
        new (
            "NoRepo_ProvIrw",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestInvalidDataProviderRw<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: Multiple       DataProvider: InvalidRw      MultiRepo_ProvIrw
        new (
            "MultiRepo_ProvIrw",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, null, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
        #region Repository: None       DataProvider: Invalid Interfaces NoRepo_ProvInvInt
        new (
            "NoRepo_ProvInvInt",
            new []
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
            (services,cache,exceptions)=>
                services.AddDataProvider<CommandEntityType, TestDataProviderRw<IEntity>>(
                    cache, null, _=>true, null, exceptions.CreateChild(nameof(ServiceCollectionHelper.AddDataProvider))),
            true,false,false
        ),
        #endregion
    
        }; }

    public AddDataProviderTests(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    public override void AddServices(IServiceCollection services, ITypeCache cache, IExceptionManager exceptions)
        => services.AddRepositories(cache, exceptions);


    [Theory, ClassData(typeof(TestArguments))]
    public override void Test(ReflectionTestOptions options, bool skipTest) 
        => base.Test(options, skipTest);


    #region test classes
    #region entities
    public class TestCommandEntity : ICommandEntity
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
    #endregion
    #region repositories

    public class TestInvalidDataProviderR<T> : IDataProviderR<T> where T : IEntity
    {
        public IQueryable<T> Get() => throw new NotImplementedException();
    }
    public class TestInvalidDataProviderRw<T> : IDataProviderRw<T> where T : IEntity
    {
        public IQueryable<T> Get() => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();

        public void Add(IEnumerable<T> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();

        public void Remove(IEnumerable<Guid> itemIds) => throw new NotImplementedException();
    }

    public class TestRepositoryR : IDataProviderR<TestCommandEntity>
    {
        public TestRepositoryR(ISourceDataProviderR<TestCommandEntity> sourceProvider)
        {

        }
        public IQueryable<TestCommandEntity> Get()
            => throw new NotImplementedException();
    }
    public class TestRepositoryRw : IDataProviderRw<TestCommandEntity>
    {
        public TestRepositoryRw()
        {

        }
        public IQueryable<TestCommandEntity> Get()
            => throw new NotImplementedException();

        public void Add(TestCommandEntity item) => throw new NotImplementedException();

        public void Add(IEnumerable<TestCommandEntity> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();

        public void Remove(IEnumerable<Guid> itemIds) => throw new NotImplementedException();
    }

    public class TestInvalidRepositoryR : ISourceDataProviderR<TestCommandEntity>
    {
        public IQueryable<TestCommandEntity> Get() => throw new NotImplementedException();
    }
    public class TestInvalidRepositoryRw : ISourceDataProviderRw<TestCommandEntity>
    {
        public IQueryable<TestCommandEntity> Get() => throw new NotImplementedException();

        public void Add(TestCommandEntity item) => throw new NotImplementedException();

        public void Add(IEnumerable<TestCommandEntity> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();

        public void Remove(IEnumerable<Guid> itemIds) => throw new NotImplementedException();
    }
    #endregion
    #region DataProvider
    public class TestDataProviderRw<T> : ISourceDataProviderRw<T> where T : IEntity
    {
        public IQueryable<T> Get() => throw new NotImplementedException();

        public void Add(T item) => throw new NotImplementedException();

        public void Add(IEnumerable<T> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();

        public void Remove(IEnumerable<Guid> itemIds) => throw new NotImplementedException();
    }
    public class TestDataProviderR<T> : ISourceDataProviderR<T> where T : IEntity
    {
        public IQueryable<T> Get() => throw new NotImplementedException();
    }

    public class TestInvalidInterfaceDataProvider<T> : ISourceDataProviderR<T>, IDataProviderRw<T>
        where T : IEntity
    {
        public IQueryable<T> Get() => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();

        public void Add(IEnumerable<T> items) => throw new NotImplementedException();

        public void Remove(Guid itemId) => throw new NotImplementedException();

        public void Remove(IEnumerable<Guid> itemIds) => throw new NotImplementedException();
    }
    #endregion
    #endregion

}
