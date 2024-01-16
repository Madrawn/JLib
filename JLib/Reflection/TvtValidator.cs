using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

public class TvtValidator : ValueValidator<Type>
{
    private readonly TypeValueType _valueType;
    public TvtValidator(TypeValueType valueType, string valueTypeName) : base(valueType.Value, valueTypeName)
    {
        _valueType = valueType;
    }

    protected override Exception? BuildException(IReadOnlyCollection<string> messages)
        => JLibAggregateException.ReturnIfNotEmpty(
            "Type validation failed",
            messages.Select(msg => new InvalidTypeException(_valueType.GetType(), _valueType.Value, msg)));

    public void ShouldBeGeneric(string? hint = null)
    {
        if (!Value.IsGenericType)
            AddError(string.Join(Environment.NewLine, "Must be Generic", hint));
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
            AddError($"It must have exactly {argumentCount} type arguments but got {Value.GenericTypeArguments.Length}");

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
            AddError($"Should implement any {typeof(TInterface).TryGetGenericTypeDefinition().FullClassName(true)}", hint);
    }
    public void ShouldImplement<TInterface>(string? hint = null)
    {
        if (!Value.ImplementsAny<TInterface>())
            AddError($"Should implement {typeof(TInterface).FullClassName(true)}", hint);
    }
    public void ShouldNotImplementAny<TInterface>(string? hint = null)
    {
        if (Value.ImplementsAny<TInterface>())
            AddError($"Should not implement {typeof(TInterface).TryGetGenericTypeDefinition().FullClassName(true)}", hint);
    }

}