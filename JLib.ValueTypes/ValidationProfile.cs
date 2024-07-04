using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;

namespace JLib.ValueTypes;

public interface IValidationProfile<in T>
{
    public IExceptionProvider Validate(T? value);
}
public sealed class ValidationProfile<TValue> : IValidationProfile<TValue>
{
    private readonly Type _owner;
    private readonly IReadOnlyCollection<MethodInfo> _validationMethods;

    public static ValidationProfile<TValue> Get(Type owner)
        => new(owner);

    public ValidationProfile(Type owner)
    {
        _owner = owner;

        _validationMethods = owner.GetMethods(BindingFlags.NonPublic | BindingFlags.Public |
                                              BindingFlags.Instance | BindingFlags.Static)
            .Where(method => method.HasCustomAttribute<ValidationAttribute>())
            .ToReadOnlyCollection();
        // todo: validate method signature
    }
    public IExceptionProvider Validate(TValue? value)
    {
        var ex = new ExceptionBuilder(_owner.FullName(true));
        // todo: writing this into an expression and compiling it would improve performance by (probably) a lot
        foreach (var method in _validationMethods)
        {
            var validatorType = method.GetParameters().Single().ParameterType;
            var validator = (IValueValidator<TValue>)Activator.CreateInstance(validatorType,
                value, typeof(TValue).FullName(true))!;
            ex.AddChild(validator);
        }

        return ex;
    }
}