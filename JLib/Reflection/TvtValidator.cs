using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

public class PropertyValidator : ValueValidator<PropertyInfo>
{
    public PropertyValidator(PropertyInfo value, string valueTypeName) : base(value, valueTypeName)
    {
    }

    public PropertyValidator ShouldHavePublicInit()
    {
        if (!Value.IsInit())
            AddError("must have public init");
        return this;
    }
    public PropertyValidator ShouldHavePublicSet()
    {
        if (Value.SetMethod?.IsPublic != true)
            AddError("must have public set");
        return this;
    }
    public PropertyValidator ShouldHavePublicGet()
    {
        if (Value.GetMethod?.IsPublic != true)
            AddError("must have public get");
        return this;
    }
}

public class TvtValidator : ValueValidator<Type>
{
    private readonly TypeValueType _valueType;

    public TvtValidator(TypeValueType valueType, string valueTypeName) : base(valueType.Value, valueTypeName)
    {
        _valueType = valueType;
    }

    protected override Exception? BuildException(IReadOnlyCollection<string> messages)
        => JLibAggregateException.ReturnIfNotEmpty(
            $"{_valueType.Value.FullClassName(true)} is not a valid {_valueType.GetType().FullClassName(true)}",
            messages.Select(msg => new InvalidTypeException(_valueType.GetType(), _valueType.Value, msg)));

    public void ValidateProperties(Func<PropertyInfo, bool> filter, Action<PropertyValidator> validator)
    {
        foreach (var property in Value.GetProperties().Where(filter))
        {
            var val = new PropertyValidator(property, ValueTypeName);
            validator(val);

            foreach (var message in val.Messages)
                AddError($"invalid property {Value.FullClassName()}.{property.Name}: {message}");
        }
    }

    public void ShouldBeGeneric(string? hint = null)
    {
        if (!Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must be Generic", hint));
    }
    public void ShouldBeSealed(string? hint = null)
    {
        if (!Value.IsSealed)
            AddError(string.Join(Environment.NewLine, "Must be Sealed", hint));
    }

    public void ShouldNotBeGeneric(string? hint = null)
    {
        if (Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must not be Generic", hint));
    }

    public void ShouldHaveNTypeArguments(int argumentCount)
    {
        ShouldBeGeneric();

        if (!Value.IsGenericType)
            AddError("Must be Generic");
        if (Value.GenericTypeArguments.Length != argumentCount)
            AddError(
                $"It must have exactly {argumentCount} type arguments but got {Value.GenericTypeArguments.Length}");
    }

    public void ShouldHaveAttribute<TAttribute>(string hint)
        where TAttribute : Attribute
    {
        if (!Value.HasCustomAttribute<TAttribute>())
            AddError($"Should have {typeof(TAttribute).FullClassName(true)}", hint);
    }

    public void ShouldImplementAny<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement any {typeof(TInterface).TryGetGenericTypeDefinition().FullClassName(true)}",
                hint);
    }

    public void ShouldImplement<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement {typeof(TInterface).FullClassName(true)}", hint);
    }

    public void ShouldNotImplementAny<TInterface>(string? hint = null)
    {
        if (Value.ImplementsAny<TInterface>())
            AddError($"Should not implement {typeof(TInterface).TryGetGenericTypeDefinition().FullClassName(true)}",
                hint);
    }
}