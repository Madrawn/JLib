using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace JLib.Tests.Reflection.ServiceCollection.AddMapDataProvider;


public class AddMapDataProviderTests:ReflectionTestBase
{
    public AddMapDataProviderTests(ITestOutputHelper testOutput) 
        : base(testOutput)
    {
    }
}
