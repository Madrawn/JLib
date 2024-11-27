﻿using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

public class TypeValidationContext : ValidationContext<Type>
{
    private readonly TypeValueType _valueType;

    public TypeValidationContext(TypeValueType valueType, Type targetType) : base(valueType.Value, targetType)
    {
        _valueType = valueType;
    }

    protected override Exception? BuildException(IReadOnlyCollection<string> messages, IReadOnlyCollection<IExceptionProvider> provider)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{_valueType.Value.FullName(true)} is not a valid {_valueType.GetType().FullName(true)}",
            messages.Select(msg => new InvalidTypeException(_valueType.GetType(), _valueType.Value, msg))
                .Concat(provider.Select(p=>p.GetException()).WhereNotNull())
            );

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

    public TypeValidationContext ShouldBeGeneric(string? hint = null)
    {
        if (!Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must be Generic", hint));
        return this;
    }
    public TypeValidationContext ShouldBeStatic(string? hint = null)
    {
        if (!Value.IsStatic())
            AddError(string.Join(Environment.NewLine, "Must be Static", hint));
        return this;
    }

    public TypeValidationContext ShouldBeSealed(string? hint = null)
    {
        if (!Value.IsSealed)
            AddError(string.Join(Environment.NewLine, "Must be Sealed", hint));
        return this;
    }

    public TypeValidationContext ShouldNotBeGeneric(string? hint = null)
    {
        if (Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must not be Generic", hint));
        return this;
    }

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
    
    public TypeValidationContext ShouldImplementAny<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement any {typeof(TInterface).TryGetGenericTypeDefinition().FullName(true)}",
                hint);
        return this;
    }

    public TypeValidationContext ShouldImplement<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement {typeof(TInterface).FullName(true)}", hint);
        return this;
    }

    public TypeValidationContext ShouldNotImplementAny<TInterface>(string? hint = null)
    {
        if (Value.ImplementsAny<TInterface>())
            AddError($"Should not implement {typeof(TInterface).TryGetGenericTypeDefinition().FullName(true)}",
                hint);
        return this;
    }
    public TypeValidationContext ShouldHaveAttribute<TAttribute>(string hint)
        where TAttribute : Attribute
    {
        if (!Value.HasCustomAttribute<TAttribute>())
            AddError($"Should have {typeof(TAttribute).FullName(true)}", hint);
        return this;
    }

    public TypeValidationContext ShouldHaveName(string name)
    {
        if (Value.Name != name)
            AddError($"must have the name '{name}'");
        return this;
    }
    public TypeValidationContext ShouldHaveNameSuffix(string nameSuffix)
    {
        if (!Value.Name.EndsWith(nameSuffix))
            AddError($"must have the nameSuffix '{nameSuffix}'");
        return this;
    }
}