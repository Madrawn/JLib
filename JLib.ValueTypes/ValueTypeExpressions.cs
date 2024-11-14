using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public static partial class ValueType
{
    /// <summary>
    /// Contains further utility methods which generate <see cref="ValueType{T}"/> Factory <see cref="LambdaExpression"/>s
    /// </summary>
    public static class FactoryExpressions
    {


        #region factory expressions

        private static readonly ConcurrentDictionary<string, LambdaExpression> ExpressionCache = new();


        /// <summary>
        /// Creates a <see cref="Expression"/>&lt;Func&lt;<typeparamref name="T"/>, <typeparamref name="TVt"/>>> which can create an instance of <typeparamref name="TVt"/> from a parameter of type <typeparamref name="T"/> which converts null arguments to null return values if and only if <paramref name="nullable"/> is true<br/>
        /// Should the <typeparamref name="TVt"/> validations fail, an exceptions will be still be thrown<br/>
        /// If <paramref name="nullable"/> is false and a null value has been passed, an exception will be thrown according to the <typeparamref name="TVt"/> validation
        /// </summary>
        /// <paramref name="nullable">If false, null values will be passed to the constructor of <typeparamref name="TVt"/>.</paramref>
        /// <typeparam name="TVt">the <see cref="ValueType{T}"/> which should be created</typeparam>
        /// <typeparam name="T">the Native type of the <typeparamref name="TVt"/></typeparam>
        /// <returns>A <see cref="LambdaExpression"/> which can create an instance of <typeparamref name="TVt"/> from a parameter of type <typeparamref name="T"/><br/>which converts null arguments to null return values if and only if <paramref name="nullable"/> is true<br/></returns>
        public static LambdaExpression ForAnyType<TVt, T>(bool nullable)
            => ForAnyType(typeof(TVt), nullable);

        /// <summary>
        /// Creates a <see cref="LambdaExpression"/> which can create an instance of the given <paramref name="valueType"/>
        /// Should the <paramref name="valueType"/> validations fail, an exceptions will be still be thrown<br/>
        /// If <paramref name="nullable"/> is false and a null value has been passed, an exception will be thrown according to the <paramref name="valueType"/> validation
        /// </summary>
        /// <paramref name="valueType">The type for which the <see cref="LambdaExpression"/> factory will be created</paramref>
        /// <paramref name="nullable">If false, null values will be passed to the <paramref name="valueType"/> constructor.</paramref>
        /// <returns>A <see cref="LambdaExpression"/> which can create an instance of <paramref name="valueType"/> from a parameter which converts null arguments to null return values if and only if <paramref name="nullable"/> is true<br/></returns>
        public static LambdaExpression ForAnyType(Type valueType, bool nullable)
        {

            var key = GetExpressionCacheKey(valueType, nullable);

            if (ExpressionCache.TryGetValue(key, out var cached))
                return cached;

            // the factory for this valueType might not be cached yet. to initialize the cache and return the expression we have to call the specific method

            var nativeType = valueType.GetAnyBaseType<ValueType<Ignored>>()?.GenericTypeArguments.Single()
                ?? throw new InvalidOperationException($"{valueType.FullName(true)} is not derived from {nameof(ValueType<Ignored>)}");

            var name =
                $"For{(nullable ? "Nullable" : "NonNullable")}{(nativeType.IsValueType ? "Struct" : "Class")}";
            var mi = typeof(FactoryExpressions).GetMethod(name, BindingFlags.Static | BindingFlags.Public);
            if (mi is null)
                throw new InvalidSetupException($"Method {name} could not be found");
            return mi.MakeGenericMethod(valueType, nativeType).Invoke(null, Array.Empty<object>()) as LambdaExpression
                         ?? throw new InvalidSetupException($"Method {name} could not be invoked");
        }

        /// <summary>
        /// Creates a <see cref="Expression"/>&lt;Func&lt;<typeparamref name="T"/>, <typeparamref name="TVt"/>>> which can create an instance of <typeparamref name="TVt"/> from a parameter of type <typeparamref name="T"/><br/>
        /// if the <typeparamref name="TVt"/> validations fails, the exceptions will still be thrown, this includes exceptions when the value is null
        /// </summary>
        /// <typeparam name="TVt">the <see cref="ValueType{T}"/> which should be created</typeparam>
        /// <typeparam name="T">the Native type of the <typeparamref name="TVt"/></typeparam>
        /// <returns>an expression which creates an instance of <typeparamref name="TVt?"/> from a parameter of type <typeparamref name="T?"/> which does not handle null values</returns>
        public static Expression<Func<T, TVt>> ForNonNullableStruct<TVt, T>()
                // note: the name follows a naming scheme and is accessed via non-nameof reflection, therefore the method must not be renamed.
                // said code is located in ForAnyType
                where TVt : ValueType<T>
                where T : struct
                => ExpressionCache.GetOrAdd(GetExpressionCacheKey<TVt>(false), _ =>
                {
                    var param = Expression.Parameter(typeof(T), "value");

                    var ctor = typeof(TVt).GetConstructor(new[] { typeof(T) });

                    if (ctor is null)
                        throw new InvalidSetupException("ctor could not be found");

                    var ctorEx = Expression.New(ctor, param);

                    return Expression.Lambda<Func<T, TVt>>(ctorEx, param);
                }).CastTo<Expression<Func<T, TVt>>>();

        /// <summary>
        /// Creates a <see cref="Expression"/>&lt;Func&lt;<typeparamref name="T"/>?, <typeparamref name="TVt"/>?>> which can create an instance of <typeparamref name="TVt"/> from a parameter of type <typeparamref name="T"/> which converts null arguments to null return values<br/>
        /// if the <typeparamref name="TVt"/> validations fails, the exceptions will still be thrown
        /// </summary>
        /// <typeparam name="TVt">the <see cref="ValueType{T}"/> which should be created</typeparam>
        /// <typeparam name="T">the Native type of the <typeparamref name="TVt"/></typeparam>
        /// <returns>an expression which creates an instance of <typeparamref name="TVt?"/> from a parameter of type <typeparamref name="T?"/> which converts null arguments to null return values</returns>
        public static Expression<Func<T?, TVt?>> ForNullableStruct<TVt, T>()
            // note: the name follows a naming scheme and is accessed via non-nameof reflection, therefore the method must not be renamed.
            // said code is located in ForAnyType
            where TVt : ValueType<T>
            where T : struct
            => ExpressionCache.GetOrAdd(GetExpressionCacheKey<TVt>(true), _ =>
            {
                var lambda = ForNonNullableStruct<TVt, T>();
                return ((Expression<Func<T?, TVt?>>)(value => value == null ? null : CtorPlaceholder<T, TVt>(value.Value))).ReplaceMethod(PlaceholderMi, lambda);
            }).CastTo<Expression<Func<T?, TVt?>>>();

        /// <summary>
        /// Creates a <see cref="Expression"/>&lt;Func&lt;<typeparamref name="T"/>, <typeparamref name="TVt"/>>> which can create an instance of <typeparamref name="TVt"/> from a parameter of type <typeparamref name="T"/><br/>
        /// if the <typeparamref name="TVt"/> validations fails, the exceptions will still be thrown, this includes exceptions when the value is null
        /// </summary>
        /// <typeparam name="TVt">the <see cref="ValueType{T}"/> which should be created</typeparam>
        /// <typeparam name="T">the Native type of the <typeparamref name="TVt"/></typeparam>
        /// <returns>an expression which creates an instance of <typeparamref name="TVt?"/> from a parameter of type <typeparamref name="T?"/> which does not handle null values</returns>
        public static Expression<Func<T, TVt>> ForNonNullableClass<TVt, T>()
            // note: the name follows a naming scheme and is accessed via non-nameof reflection, therefore the method must not be renamed.
            // said code is located in ForAnyType
            where TVt : ValueType<T>
            where T : class
            => ExpressionCache.GetOrAdd(GetExpressionCacheKey<TVt>(false), _ =>
            {
                var param = Expression.Parameter(typeof(T), "value");

                var ctor = typeof(TVt).GetConstructor(new Type[] { typeof(T) });

                if (ctor is null)
                    throw new InvalidSetupException("ctor could not be found");

                var ctorEx = Expression.New(ctor, param);

                return Expression.Lambda<Func<T, TVt>>(ctorEx, param);
            }).CastTo<Expression<Func<T, TVt>>>();


        /// <summary>
        /// Creates a <see cref="Expression"/>&lt;Func&lt;<typeparamref name="T"/>?, <typeparamref name="TVt"/>?>> which can create an instance of <typeparamref name="TVt"/> from a parameter of type <typeparamref name="T"/> which converts null arguments to null return values<br/>
        /// if the <typeparamref name="TVt"/> validations fails, the exceptions will still be thrown
        /// </summary>
        /// <typeparam name="TVt">the <see cref="ValueType{T}"/> which should be created</typeparam>
        /// <typeparam name="T">the Native type of the <typeparamref name="TVt"/></typeparam>
        /// <returns>an expression which creates an instance of <typeparamref name="TVt?"/> from a parameter of type <typeparamref name="T?"/> which converts null arguments to null return values</returns>
        public static Expression<Func<T?, TVt?>> ForNullableClass<TVt, T>()
            // note: the name follows a naming scheme and is accessed via non-nameof reflection, therefore the method must not be renamed.
            // said code is located in ForAnyType
            where TVt : ValueType<T>
            where T : class
            => ExpressionCache.GetOrAdd(GetExpressionCacheKey<TVt>(true), _ =>
            {
                var lambda = ForNonNullableClass<TVt, T>();
                return ((Expression<Func<T?, TVt?>>)(value => value == null ? null : CtorPlaceholder<T, TVt>(value))).ReplaceMethod(PlaceholderMi, lambda);
            }).CastTo<Expression<Func<T?, TVt?>>>();
        private static readonly MethodInfo PlaceholderMi = typeof(FactoryExpressions).GetMethod(nameof(CtorPlaceholder), BindingFlags.Static | BindingFlags.NonPublic)
                                                            ?? throw new InvalidSetupException("Ctor placeholder could not be found");
        private static TVt CtorPlaceholder<T, TVt>(T value)
            where TVt : ValueType<T>
            => throw new InvalidSetupException("this should have been replaced");

        #endregion

    }

}