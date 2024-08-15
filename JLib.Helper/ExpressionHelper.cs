using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JLib.Helper;

public static class ExpressionHelper
{
    /// <summary>
    /// Converts the specified expression to a nullable expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A nullable expression.</returns>
    public static Expression<Func<TSource, TKey?>> ToNullable<TSource, TKey>(
        this Expression<Func<TSource, TKey>> expression)
        where TKey : struct
    {
        var param = Expression.Parameter(typeof(TSource), expression.Parameters[0].Name);
        var body = Expression.Convert(expression.Body, typeof(TKey?));
        return Expression.Lambda<Func<TSource, TKey?>>(body, param);
    }

    /// <summary>
    /// Gets the property information from the specified property lambda expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="propertyLambda">The property lambda expression.</param>
    /// <returns>The property information.</returns>
    public static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> propertyLambda)
    {
        Type type = typeof(TSource);

        MemberExpression member = propertyLambda.Body switch
        {
            MemberExpression directMember => directMember,
            UnaryExpression { Operand: MemberExpression indirectMember } => indirectMember,
            _ => throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.")
        };

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

        if (propInfo.ReflectedType is null)
            throw new ArgumentException($"Expression '{propertyLambda}' has no ReflectedType.");

        if (type != propInfo.ReflectedType && (propInfo.ReflectedType.IsInterface &&
                                               !type.Implements(propInfo.ReflectedType))
                                           && !type.IsSubclassOf(propInfo.ReflectedType))
            throw new ArgumentException(
                $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

        return propInfo;
    }

    /// <summary>
    /// Replaces all calls to the specified <paramref name="method"/> with the specified <paramref name="replacementExpression"/> in the given <paramref name="inputExpression"/> of type <typeparamref name="T"/> using an <see cref="ExpressionVisitor"/>.<br/>
    ///     <list type="bullet">
    ///         <item>must be a <see cref="LambdaExpression"/></item>
    ///         <item>all parameter types must match those of <paramref name="method"/></item>
    ///         <item>return type must match that of <paramref name="method"/></item>
    ///         <item>return type must match that of <paramref name="method"/></item>
    ///     </list>
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="inputExpression"/> and the Return Value.</typeparam>
    /// <param name="inputExpression">The expression to be edited.</param>
    /// <param name="method">The method to be replaced.</param>
    /// <param name="replacementExpression">The <see cref="LambdaExpression"/> to replace the <paramref name="method"/> with. <br/></param> 
    /// <returns>An expression with all occurrences of the <paramref name="method"/> replaced with the <paramref name="replacementExpression"/> expression.</returns>
    public static Expression<T> ReplaceMethod<T>(this Expression<T> inputExpression, MethodInfo method,
        LambdaExpression replacementExpression)
    {
        var visitor = new ReplaceExpressionVisitor(method, replacementExpression);
        var ex = visitor.Visit(inputExpression);
        return (Expression<T>)ex;
    }

    #region expression visitors

    /// <summary>
    /// replaces all occurrences of method calls to <see cref="_replace"/> with the given expression. <see cref="_with"/><br/>
    /// Uses the <see cref="ParameterVisitor"/> to replace the <see cref="LambdaExpression.Parameters"/> of the <see cref="_with"/> Expression with the <see cref="MethodCallExpression.Arguments"/> values of the specific <see cref="_replace"/> insatnce.
    /// </summary>
    private sealed class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly MethodInfo _replace;
        private readonly Expression _with;

        public ReplaceExpressionVisitor(MethodInfo replace, Expression with)
        {
            _replace = replace;
            _with = with;

            switch (with)
            {
                case LambdaExpression lambda:
                    // check parameter types
                    var parameterPairs = lambda.Parameters.Zip(replace.GetParameters(),
                        (lambdaPar, replacePar) => new { lambdaPar, replacePar })
                        .ToArray();
                    if (parameterPairs.Any(x =>
                            x.lambdaPar.Type.IsAssignableTo(x.replacePar.ParameterType) == false
                            && !x.replacePar.ParameterType.IsGenericParameter // type parameters are not resolved yet, therefore string can not be assigned to the unknown type T (it is not defined yet)
                            )
                        )
                        throw new ArgumentException($"parameter type mismatch: " +
                                                    $"found: ({string.Join(", ", parameterPairs.Select(x => x.lambdaPar.Type.Name + " " + x.lambdaPar.Name))}) | " +
                                                    $"compare:  {replace.ToInfoString()}");
                    if (replace.ReturnType.IsGenericParameter
                        ? lambda.ReturnType.IsAssignableTo(replace.ReturnType)
                        : lambda.ReturnType != replace.ReturnType)
                        throw new ArgumentException("return type mismatch");
                    break;
                default:
                    throw new NotSupportedException(
                        $"expressions of type {_with.GetType().FullName()} are not supported");
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if ((node.Method.IsGenericMethod ? node.Method.GetGenericMethodDefinition() : node.Method) != _replace)
                return base.VisitMethodCall(node);

            switch (_with)
            {
                case LambdaExpression lambda:
                    // we are still referring to the parameters of the method info. we have to replace all occurrences with the expression argument. since this happens inside the body, another visitor seems to be the best solution.
                    var visitor = new ParameterVisitor(node.Arguments.Zip(lambda.Parameters, (arg, par) => new { arg, par })
                        .ToDictionary(x => x.par, x => x.arg));
                    var body = visitor.Visit(lambda.Body);
                    return body;
                default:
                    throw new NotSupportedException(
                        $"expressions of type {_with.GetType().FullName()} are not supported");
            }
        }
    }

    /// <summary>
    /// replaces all occurrences of a parameter with a given expression.
    /// </summary>
    private sealed class ParameterVisitor : ExpressionVisitor
    {
        private readonly IReadOnlyDictionary<ParameterExpression, Expression> _parameters;

        public ParameterVisitor(IReadOnlyDictionary<ParameterExpression, Expression> parameters)
        {
            _parameters = parameters;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var argument = _parameters.GetValueOrDefault(node);
            return argument ?? base.VisitParameter(node);
        }
    }

    #endregion
}
