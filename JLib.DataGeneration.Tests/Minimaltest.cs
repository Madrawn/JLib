using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataGeneration.Tests;
public class MinimalTest
{
    public class TestEntity
    {
        public int Id { get; init; }
        public string Value { get; set; } = "";
    }

    public class TestDataPackage : DataPackage
    {
        public TestDataPackage()
        {
        }
    }

    public class TestRepository
    {
        private List<TestEntity> _entities = new();
        public void Add(TestEntity entity)
            => _entities.Add(entity);
        public void Add(IReadOnlyCollection<TestEntity> entities)
            => _entities.AddRange(entities);
    }

    public MinimalTest()
    {
        var services = new ServiceCollection()
            .AddSingleton<TestRepository>();
        var provider = services.BuildServiceProvider();

        var packages = DataPackageManager.ApplyPackages(provider, p => p.Include<TestDataPackage>());

    }

}
