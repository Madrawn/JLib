using JLib.Exceptions;

namespace JLib.ValueTypes;

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