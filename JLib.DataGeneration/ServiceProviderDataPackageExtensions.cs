using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataGeneration;

// content created using
// var sb = new StringBuilder();
// for (int i = 1; i< 20; i++)
// {
//     sb.AppendLine("    /// <summary>")
//         .AppendLine("    /// loads the given <see cref=\"DataPackage\"/>. should only be used inside the <see cref=\"DataPackage\"/> ctor.")
//         .AppendLine("    /// </summary>")
//         .Append("    public static IServiceProvider LoadDataProviders<").AppendJoin(", ", Enumerable.Range(1, i).Select(i => $"TDp{i}")).AppendLine(">(this IServiceProvider serviceProvider)")
//         .AppendJoin(Environment.NewLine, Enumerable.Range(1, i).Select(i => $"        where TDp{i} : DataPackage")).AppendLine()
//         .AppendLine("    {")
//         .AppendJoin(Environment.NewLine, Enumerable.Range(1, i).Select(i => $"        serviceProvider.GetRequiredService<TDp{i}>();")).AppendLine()
//         .AppendLine("        return serviceProvider;")
//         .AppendLine("    }");
// }
// Console.WriteLine(sb)

/// <summary>
/// extension methods for <see cref="IServiceProvider"/> when working with <see cref="DataPackage"/>s.
/// </summary>
public static class ServiceProviderDataPackageExtensions
{
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        serviceProvider.GetRequiredService<TDp14>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        serviceProvider.GetRequiredService<TDp14>();
        serviceProvider.GetRequiredService<TDp15>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        serviceProvider.GetRequiredService<TDp14>();
        serviceProvider.GetRequiredService<TDp15>();
        serviceProvider.GetRequiredService<TDp16>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        serviceProvider.GetRequiredService<TDp14>();
        serviceProvider.GetRequiredService<TDp15>();
        serviceProvider.GetRequiredService<TDp16>();
        serviceProvider.GetRequiredService<TDp17>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
        where TDp18 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        serviceProvider.GetRequiredService<TDp14>();
        serviceProvider.GetRequiredService<TDp15>();
        serviceProvider.GetRequiredService<TDp16>();
        serviceProvider.GetRequiredService<TDp17>();
        serviceProvider.GetRequiredService<TDp18>();
        return serviceProvider;
    }
    /// <summary>
    /// loads the given <see cref="DataPackage"/>. should only be used inside the <see cref="DataPackage"/> ctor.
    /// </summary>
    public static IServiceProvider LoadDataProviders<TDp1, TDp2, TDp3, TDp4, TDp5, TDp6, TDp7, TDp8, TDp9, TDp10, TDp11, TDp12, TDp13, TDp14, TDp15, TDp16, TDp17, TDp18, TDp19>(this IServiceProvider serviceProvider)
        where TDp1 : DataPackage
        where TDp2 : DataPackage
        where TDp3 : DataPackage
        where TDp4 : DataPackage
        where TDp5 : DataPackage
        where TDp6 : DataPackage
        where TDp7 : DataPackage
        where TDp8 : DataPackage
        where TDp9 : DataPackage
        where TDp10 : DataPackage
        where TDp11 : DataPackage
        where TDp12 : DataPackage
        where TDp13 : DataPackage
        where TDp14 : DataPackage
        where TDp15 : DataPackage
        where TDp16 : DataPackage
        where TDp17 : DataPackage
        where TDp18 : DataPackage
        where TDp19 : DataPackage
    {
        serviceProvider.GetRequiredService<TDp1>();
        serviceProvider.GetRequiredService<TDp2>();
        serviceProvider.GetRequiredService<TDp3>();
        serviceProvider.GetRequiredService<TDp4>();
        serviceProvider.GetRequiredService<TDp5>();
        serviceProvider.GetRequiredService<TDp6>();
        serviceProvider.GetRequiredService<TDp7>();
        serviceProvider.GetRequiredService<TDp8>();
        serviceProvider.GetRequiredService<TDp9>();
        serviceProvider.GetRequiredService<TDp10>();
        serviceProvider.GetRequiredService<TDp11>();
        serviceProvider.GetRequiredService<TDp12>();
        serviceProvider.GetRequiredService<TDp13>();
        serviceProvider.GetRequiredService<TDp14>();
        serviceProvider.GetRequiredService<TDp15>();
        serviceProvider.GetRequiredService<TDp16>();
        serviceProvider.GetRequiredService<TDp17>();
        serviceProvider.GetRequiredService<TDp18>();
        serviceProvider.GetRequiredService<TDp19>();
        return serviceProvider;
    }
}
