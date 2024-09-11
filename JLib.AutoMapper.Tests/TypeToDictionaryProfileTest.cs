using AutoMapper;
using FluentAssertions;
using Xunit;

namespace JLib.AutoMapper.Tests;


public class TypeToDictionaryProfileTest
{
    private class Demo
    {
        public string Name { get; set; } = "initial";
        public int Value { get; set; } = 2;
        public string? Null { get; set; } = null;
    }
    [Fact]
    public void Test()
    {

        var mapper = new MapperConfiguration(x => x.AddProfile<TypeToDictionaryProfile<Demo>>()).CreateMapper();
        var res = mapper.Map<Dictionary<string, string?>>(new Demo() { });
        res.Should().HaveCount(3)
            .And.Equal(new Dictionary<string, string?>()
            {
                {nameof(Demo.Name),"initial"},
                {nameof(Demo.Value),"2"},
                {nameof(Demo.Null),null}
            });
    }
}
