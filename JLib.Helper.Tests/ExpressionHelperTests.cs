using System.Linq.Expressions;
using FluentAssertions;
using Xunit;

namespace JLib.Helper.Tests;

public class ExpressionHelperTests
{

    private string Replace(int replaceParameter) => "replace me " + replaceParameter;
    [Fact]
    public void NonGeneric()
    {
        Expression<Func<int, string>> ex = i => Replace(i);
        var mi = this.GetType().GetMethod(nameof(Replace), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?? throw new("method not found");
        var exRes = ex.ReplaceMethod(mi, (int i) => "replaced " + i);
        exRes.Compile()(1).Should().Be("replaced 1");

    }




    private string Replace2<T>(T replaceParameter) => "replace me " + replaceParameter;
    [Fact]
    public void GenericMethod()
    {
        // from: (string t)=>Replace2<string>(t)
        //   to: (string t)=>"replaced "+t
        Expression<Func<string, string>> ex = str => Replace2(str);
        var mi = GetType().GetMethod(nameof(Replace2),
                     System.Reflection.BindingFlags.NonPublic
                     | System.Reflection.BindingFlags.Instance)
                 ?? throw new("method not found");
        var exRes = ex.ReplaceMethod(mi, (string str) => "replaced " + str);
        exRes.Compile()("par").Should().Be("replaced par");
    }




    private class TestClass<T>
    {
        public string Test(T t)
        {
            Expression<Func<T, string>> ex = str => Replace3(str);
            var mi = GetType().GetMethod(nameof(Replace3),
                         System.Reflection.BindingFlags.NonPublic
                         | System.Reflection.BindingFlags.Instance)
                     ?? throw new("method not found");
            var exRes = ex.ReplaceMethod(mi, (string str) => "replaced " + str);
            return exRes.Compile()(t);

        }
        private string Replace3(T replaceParameter) => "replace me " + replaceParameter;

    }
    [Fact]
    public void GenericClass()
    {
        new TestClass<string>().Test("par").Should().Be("replaced par");
    }


}