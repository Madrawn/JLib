using System.Reflection;

using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

public sealed class TypeValidationContext : ValidationContext<Type>
{
    private readonly TypeValueType _valueType;

    public TypeValidationContext(TypeValueType valueType, Type targetType) : base(valueType.Value, targetType)
    {
        _valueType = valueType;
    }

    /// <summary>
    /// <inheritdoc cref="ValidationContext{TValue}.BuildException"/>
    /// </summary>
    protected override Exception? BuildException(IReadOnlyCollection<string> messages, IReadOnlyCollection<IExceptionProvider> provider)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{_valueType.Value.FullName(true)} is not a valid {_valueType.GetType().FullName(true)}",
            messages.Select(msg => new InvalidTypeException(_valueType.GetType(), _valueType.Value, msg))
                .Concat(provider.Select(p => p.GetException()).WhereNotNull())
            );

    /// <summary>
    /// Validates all properties of the <see cref="Type"/> which match the given <paramref name="filter"/>
    /// </summary>
    public TypeValidationContext ValidateProperties(Func<PropertyInfo, bool> filter, Action<ValidationContext<PropertyInfo>> validator)
    {
        foreach (var property in Value.GetProperties().Where(filter))
        {
            var val = new ValidationContext<PropertyInfo>(property, TargetType);
            validator(val);
            AddSubValidators(val);
        }
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be generic
    /// </summary>
    public TypeValidationContext ShouldBeGeneric(string? hint = null)
    {
        if (!Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must be Generic", hint));
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be static
    /// </summary>
    public TypeValidationContext ShouldBeStatic(string? hint = null)
    {
        if (!Value.IsStatic())
            AddError(string.Join(Environment.NewLine, "Must be Static", hint));
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be sealed
    /// </summary>
    public TypeValidationContext ShouldBeSealed(string? hint = null)
    {
        if (!Value.IsSealed)
            AddError(string.Join(Environment.NewLine, "Must be Sealed", hint));
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be a generic type
    /// </summary>
    /// <param name="hint"></param>
    /// <returns></returns>
    public TypeValidationContext ShouldNotBeGeneric(string? hint = null)
    {
        if (Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must not be Generic", hint));
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be a generic type and have exactly <paramref name="argumentCount"/> type arguments
    /// </summary>
    public TypeValidationContext ShouldHaveNTypeArguments(int argumentCount)
    {
        ShouldBeGeneric();

        if (!Value.IsGenericType)
            AddError("Must be Generic");
        if (Value.GenericTypeArguments.Length != argumentCount)
            AddError(
                $"It must have exactly {argumentCount} type arguments but got {Value.GenericTypeArguments.Length}");
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to implement <typeparamref name="TInterface"/> ignoring all of its type arguments
    /// </summary>
    public TypeValidationContext ShouldImplementAny<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement any {typeof(TInterface).TryGetGenericTypeDefinition().FullName(true)}",
                hint);
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to implement <typeparamref name="TInterface"/> with the given type arguments
    /// </summary>
    public TypeValidationContext ShouldImplement<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement {typeof(TInterface).FullName(true)}", hint);
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> not to implement <typeparamref name="TInterface"/> ignoring all of its type arguments
    /// </summary>
    public TypeValidationContext ShouldNotImplementAny<TInterface>(string? hint = null)
    {
        if (Value.ImplementsAny<TInterface>())
            AddError($"Should not implement {typeof(TInterface).TryGetGenericTypeDefinition().FullName(true)}",
                hint);
        return this;
    }

    /// <summary>
    /// Expects the <see cref="Type"/> to be decorated with <typeparamref name="TAttribute"/>
    /// </summary>
    public TypeValidationContext ShouldHaveAttribute<TAttribute>(string? hint = null)
        where TAttribute : Attribute
    {
        if (!Value.HasCustomAttribute<TAttribute>())
            AddError($"Should have {typeof(TAttribute).FullName(true)}", hint);
        return this;
    }

    /// <summary>
    /// expects the <see cref="MemberInfo.Name"/> to <see cref="string.Equals(string?,StringComparison)"/> <paramref name="name"/> with the <paramref name="comparisonType"/>
    /// </summary>
    public TypeValidationContext ShouldHaveName(string name, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (Value.Name.Equals(name, comparisonType))
            AddError($"must have the name '{name}'");
        return this;
    }

    /// <summary>
    /// expects the <see cref="MemberInfo.Name"/> to end with the given <paramref name="nameSuffix"/>
    /// </summary>
    public TypeValidationContext ShouldHaveNameSuffix(string nameSuffix)
    {
        if (!Value.Name.EndsWith(nameSuffix))
            AddError($"must have the nameSuffix '{nameSuffix}'");
        return this;
    }
}