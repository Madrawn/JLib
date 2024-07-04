using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

/// <summary>
/// This property is used to validate this value type
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ValueTypeValidatorAttribute : Attribute { }


/// <summary>
/// Contains Utility methods to validate value types
/// </summary>
public static class ValueType
{
    private class ConstantErrorValidationProfile<T> : IValidationProfile<T>
    {
        private readonly IExceptionProvider _ex;
        public ConstantErrorValidationProfile(IExceptionProvider ex)
        {
            _ex = ex;
        }
        public IExceptionProvider Validate(T? value) => _ex;
    }
    private record ValidatorContainer(object Value);
    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<ValidatorContainer>> Validators = new();

    /// <summary>
    /// checks, whether the given <paramref name="value"/> is a valid <typeparamref name="TVt"/>
    /// </summary>
    /// <typeparam name="TVt">The specific <see cref="ValueType{T}"/> to validate the <paramref name="value"/> for</typeparam>
    /// <typeparam name="T">The native value of the <see cref="ValueType{T}"/></typeparam>
    /// <param name="value">the value to validate</param>
    /// <returns>an <see cref="IExceptionProvider"/> containing all validation errors. Use <see cref="IExceptionProvider.HasErrors"/> to check if the value is valid</returns>
    public static IExceptionProvider Validate<TVt, T>(T value)
        where TVt : ValueType<T> => Validate(value, typeof(TVt));

    /// <summary>
    /// checks, whether the given <paramref name="value"/> is a valid value for the given <paramref name="valueType"/>
    /// </summary>
    /// <typeparam name="T">The native value of the <see cref="ValueType{T}"/></typeparam>
    /// <param name="value">the value to validate</param>
    /// <param name="valueType">The specific <see cref="ValueType{T}"/> to validate the <paramref name="value"/> for</param>
    /// <returns>an <see cref="IExceptionProvider"/> containing all validation errors. Use <see cref="IExceptionProvider.HasErrors"/> to check if the value is valid</returns>
    public static IExceptionProvider Validate<T>(T value, Type valueType)
    {
        var validators = Validators
            .GetOrAdd(valueType, vt =>
            {
                var member = vt
                    .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var props =
                    member.OfType<PropertyInfo>()
                    .Where(p =>
                        p.PropertyType.ImplementsAny<IValidationProfile<T>>()
                        || p.PropertyType.HasCustomAttribute<ValueTypeValidatorAttribute>())
                    .ToReadOnlyCollection();

                // todo: validate properties in typeCache to prevent runtime issues
                var setupIssues = new ExceptionBuilder("Validator Setup Issues");
                foreach (var property in props)
                {
                    using var propIssues = setupIssues
                        .CreateChild($"Property {property.DeclaringType?.FullName()}.{property.Name}");
                    if (property.HasCustomAttribute<ValueTypeValidatorAttribute>() == false)
                        propIssues.Add($"missing {nameof(ValueTypeValidatorAttribute)}");

                    if (property.PropertyType != typeof(IValidationProfile<T>))
                        propIssues.Add($"property must be of type {typeof(IValidationProfile<T>).FullName()}");
                    if (property.SetMethod != null)
                        propIssues.Add("setter is not allowed");
                    if (property.GetGetMethod()?.IsStatic != true)
                        propIssues.Add("getter must be static");
                    if (property.IsNullable())
                        propIssues.Add("property must not be nullable");
                    if (property.GetValue(null) == null)
                        propIssues.Add("property value must not be null");
                }

                return props
                    .Select(prop => prop.GetValue(null) as IValidationProfile<T>)
                    .WhereNotNull()
                    .Prepend(new ConstantErrorValidationProfile<T>(setupIssues))
                    .Select(validator => new ValidatorContainer(validator))
                    .ToReadOnlyCollection();
            });

        var exceptions = new ExceptionBuilder($"{value} is not a valid {valueType.FullName()}");
        exceptions.AddChildren(validators.Select(v => v.Value.As<IValidationProfile<T>>()?.Validate(value)).WhereNotNull());
        return exceptions;
    }
}

/// <summary>
/// Base class for all value types<br/>
/// use <seealso cref="ValueTypeJsonConverterFactory"/> for <seealso cref="JsonSerializer"/> conversions<br/>
/// uses automapper to instantiate the <see cref="ValueType{T}"/>.<br/>
/// <seealso cref="ValueTypeProfile"/> can generate all required maps and is included in <seealso cref="AutoMapperHelper.AddProfiles"/> as long as the <seealso cref="ITypeCache"/> contains said <seealso cref="ValueType{T}"/>
/// </summary>
public abstract record ValueType<T>(T Value)
{
    public virtual void Deconstruct(out T value)
    {
        value = Value;
    }

    /// <summary>
    /// Helper to create a tryGet method for a derived class
    /// </summary>
    /// <typeparam name="TVt">the type of the derived valueType</typeparam>
    /// <typeparam name="TValidator">the type of the validator used</typeparam>
    /// <param name="value">the value of the new valueType instance</param>
    /// <param name="tvtFactory">a method which creates a instance of an already validated value. Value validation can be skipped when creating the new Vt by using the "base(value,false)" constructor overload</param>
    /// <param name="validatorFactory">creates a new validator for the given value</param>
    /// <param name="validator">the same validator which is used to validate the valueType.</param>
    /// <returns>the valueType if the value is valid, otherwise null</returns>
    /// <returns></returns>
    protected static TVt? TryGet<TVt, TValidator>(
        T? value, Func<T, TValidator> validatorFactory, Action<TValidator> validator, Func<T, TVt> tvtFactory)
        where TVt : StringValueType
        where TValidator : IExceptionProvider
    {
        var val = validatorFactory(value!);
        validator(val);
        IExceptionProvider exProv = val;
        return exProv.GetException() is null
            ? tvtFactory(value!)
            : null;
    }
}