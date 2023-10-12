using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib;

public class TvtValidator : IExceptionProvider
{
    private readonly ITypeValueType _typeValueType;
    private Type Value => _typeValueType.Value;
    private readonly List<string> _messages = new();
    public TvtValidator(ITypeValueType typeValueType)
    {
        _typeValueType = typeValueType;
    }
    public void AddError(string message, string? hint = null)
    {
        if (hint != null)
            message += $" this might be resolved by {hint}";
        _messages.Add(message);
    }

    Exception? IExceptionProvider.GetException()
        => JLibAggregateException.ReturnIfNotEmpty(
            "Type validation failed",
            _messages.Select(msg => new InvalidTypeException(_typeValueType.GetType(), _typeValueType.Value, msg)));

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