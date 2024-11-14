
// generated using 
// StringBuilder sb = new();
// 
// sb.AppendLine(@"using Microsoft.Extensions.DependencyInjection;
// 
// namespace JLib.DependencyInjection;
// 
// public static class ServiceProviderHelper
// {
//     public static TSrvContainer GetServiceContainer<TSrvContainer>(this IServiceProvider provider)
//         where TSrvContainer : ServiceContainer, new()
//     {
//         var c = new TSrvContainer();
//         c.Init(provider);
//         return c;
//     }
// 
// ");
// 
// for (int i = 1; i <= 20; i++)
// {
// 
//     sb.AppendLine("    /// <returns>the requested services</returns>")
//         .AppendLine("public static IServiceProvider GetRequiredServices")
//         .Append("<")
//         .AppendJoin(", ", Enumerable.Range(0, i).Select(x => $"T{x}"))
//         .AppendLine(">")
//         .Append("(this IServiceProvider provider,")
//         .AppendJoin(", ", Enumerable.Range(0, i).Select(x => $"out T{x} s{x}"))
//         .AppendLine(")")
//         .AppendJoin(Environment.NewLine, Enumerable.Range(0, i).Select(x => $" where T{x} : notnull"))
//         .AppendLine("{")
//         .AppendJoin(Environment.NewLine, Enumerable.Range(0, i).Select(x => $"        s{x} = provider.GetRequiredService<T{x}>();"))
//         .AppendLine()
//         .AppendLine("        return provider;")
//         .AppendLine("    }").AppendLine();
// }
// sb.AppendLine("}");
// 
// 
// Console.WriteLine( sb.ToString())


using Microsoft.Extensions.DependencyInjection;

namespace JLib.DependencyInjection;

public static partial class ServiceProviderExtensions
{

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0>
    (this IServiceProvider provider, out T0 s0)
     where T0 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1>
    (this IServiceProvider provider, out T0 s0, out T1 s1)
     where T0 : notnull
     where T1 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13, out T14 s14)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
     where T14 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        s14 = provider.GetRequiredService<T14>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13, out T14 s14, out T15 s15)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
     where T14 : notnull
     where T15 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        s14 = provider.GetRequiredService<T14>();
        s15 = provider.GetRequiredService<T15>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13, out T14 s14, out T15 s15, out T16 s16)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
     where T14 : notnull
     where T15 : notnull
     where T16 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        s14 = provider.GetRequiredService<T14>();
        s15 = provider.GetRequiredService<T15>();
        s16 = provider.GetRequiredService<T16>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13, out T14 s14, out T15 s15, out T16 s16, out T17 s17)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
     where T14 : notnull
     where T15 : notnull
     where T16 : notnull
     where T17 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        s14 = provider.GetRequiredService<T14>();
        s15 = provider.GetRequiredService<T15>();
        s16 = provider.GetRequiredService<T16>();
        s17 = provider.GetRequiredService<T17>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13, out T14 s14, out T15 s15, out T16 s16, out T17 s17, out T18 s18)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
     where T14 : notnull
     where T15 : notnull
     where T16 : notnull
     where T17 : notnull
     where T18 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        s14 = provider.GetRequiredService<T14>();
        s15 = provider.GetRequiredService<T15>();
        s16 = provider.GetRequiredService<T16>();
        s17 = provider.GetRequiredService<T17>();
        s18 = provider.GetRequiredService<T18>();
        return provider;
    }

    /// <returns>the requested services</returns>
    public static IServiceProvider GetRequiredServices
    <T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>
    (this IServiceProvider provider, out T0 s0, out T1 s1, out T2 s2, out T3 s3, out T4 s4, out T5 s5, out T6 s6, out T7 s7, out T8 s8, out T9 s9, out T10 s10, out T11 s11, out T12 s12, out T13 s13, out T14 s14, out T15 s15, out T16 s16, out T17 s17, out T18 s18, out T19 s19)
     where T0 : notnull
     where T1 : notnull
     where T2 : notnull
     where T3 : notnull
     where T4 : notnull
     where T5 : notnull
     where T6 : notnull
     where T7 : notnull
     where T8 : notnull
     where T9 : notnull
     where T10 : notnull
     where T11 : notnull
     where T12 : notnull
     where T13 : notnull
     where T14 : notnull
     where T15 : notnull
     where T16 : notnull
     where T17 : notnull
     where T18 : notnull
     where T19 : notnull
    {
        s0 = provider.GetRequiredService<T0>();
        s1 = provider.GetRequiredService<T1>();
        s2 = provider.GetRequiredService<T2>();
        s3 = provider.GetRequiredService<T3>();
        s4 = provider.GetRequiredService<T4>();
        s5 = provider.GetRequiredService<T5>();
        s6 = provider.GetRequiredService<T6>();
        s7 = provider.GetRequiredService<T7>();
        s8 = provider.GetRequiredService<T8>();
        s9 = provider.GetRequiredService<T9>();
        s10 = provider.GetRequiredService<T10>();
        s11 = provider.GetRequiredService<T11>();
        s12 = provider.GetRequiredService<T12>();
        s13 = provider.GetRequiredService<T13>();
        s14 = provider.GetRequiredService<T14>();
        s15 = provider.GetRequiredService<T15>();
        s16 = provider.GetRequiredService<T16>();
        s17 = provider.GetRequiredService<T17>();
        s18 = provider.GetRequiredService<T18>();
        s19 = provider.GetRequiredService<T19>();
        return provider;
    }
}

