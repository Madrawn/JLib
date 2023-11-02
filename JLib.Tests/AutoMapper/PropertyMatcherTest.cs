using System.Diagnostics.CodeAnalysis;
using JLib.AutoMapper;
using JLib.Data;
using JLib.Exceptions;
using JLib.FactoryAttributes;
using JLib.Helper;
using JLib.HotChocolate;
using JLib.Reflection;
using JLib.Testing;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Tests.AutoMapper;
public class PropertyMatcherTest
{
    [Fact]
    public void MappingTest()
    {
        IExceptionManager exceptions = new ExceptionManager("tests");
        var services = new ServiceCollection()
            .AddTypeCache(
                out var typeCache,
                exceptions,
                JLibTypePackage.Instance,
                TypePackage.GetNested<PropertyMatcherTest>()
                );
    }










    #region test classes
    [NotAbstract, IsClass, IsAssignableTo(typeof(TestGdo))]
    public record TestGdoType(Type Value) : DataObjectType(Value);
    public class TestGdo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public Guid OtherEntityId { get; set; }
    }

    public class TestRoe
    {
        public Guid BusId { get; set; }
        public string BusName { get; set; } = "";
        public Guid OtherEntity { get; set; }
    }
    [NotAbstract, IsClass, IsAssignableTo(typeof(Api_Test))]
    public record TestApiType(Type Value) : CommandEntityType(Value);

    [SuppressMessage("ReSharper", "all")]
    public class Api_Test
    {
        public Guid Id { get; set; }
        public Relation OtherEntity { get; set; } = new();
        public string Bus_Name { get; set; } = "";
    }
    public class Relation
    {
        public Guid Id { get; set; }
    }



    [NotAbstract, IsClass, IsAssignableTo(typeof(TestRoe))]
    public record TestRoeType(Type Value) : CommandEntityType(Value), IMappedDataObjectType
    {
        public TestGdoType QueryObject => Navigate(cache =>
            typeof(TestGdo).CastValueType<TestGdoType>(cache)
        );
        public TestApiType Api => Navigate(cache =>
            typeof(TestApiType).CastValueType<TestApiType>(cache)
        );
        public void Initialize(ITypeCache cache, IExceptionManager exceptions)
        {
            MappingInfo = new ExplicitTypeMappingInfo[]
            {
                new(this, QueryObject,
                    MappingDataProviderMode.Read,
                    new IPropertyResolver[]
                    {
                        new PropertyPrefix("Bus"),
                        PropertyResolver.IgnoreCase
                    },
                    new IPropertyResolver[]
                    {
                        new PropertySuffix("Id"),
                        PropertyResolver.IgnoreCase
                    }),
                //api has more properties that roe
                new(this, Api,
                    MappingDataProviderMode.Disabled,
                    new IPropertyResolver[]
                    {
                        new PropertyPrefix("Bus"),
                        PropertyResolver.IgnoreCase
                    },
                    new IPropertyResolver[]
                    {
                        new PropertyPrefixSeparator("_"),
                        PropertyResolver.IgnoreCase
                    }),

                //too many name derivations
                new(Api, QueryObject,
                    MappingDataProviderMode.Disabled,
                    new IPropertyResolver[]
                    {
                        new PropertyPrefixSeparator("_"),
                        PropertyResolver.IgnoreCase
                    },
                    new IPropertyResolver[]
                    {
                        new PropertySuffix("Id"),
                        PropertyResolver.IgnoreCase
                    })
            };
        }

        public ExplicitTypeMappingInfo[] MappingInfo { get; private set; } = Array.Empty<ExplicitTypeMappingInfo>();
    }
    #endregion
}
