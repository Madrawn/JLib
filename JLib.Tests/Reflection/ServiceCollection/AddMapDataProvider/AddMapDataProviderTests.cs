using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace JLib.Tests.Reflection.ServiceCollection.AddMapDataProvider;

public class AddMapDataProviderTests : ReflectionTestBase
{
    public class TestArguments : ReflectionTestArguments
    {
        protected override IEnumerable<ReflectionTestOptions> Options { get; } = new ReflectionTestOptions[]
        {

        };
    }
    public AddMapDataProviderTests(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }
    [Theory, ClassData(typeof(TestArguments))]
    public override void Test(ReflectionTestOptions options, bool skipTest)
        => base.Test(options, skipTest);
}
