using JLib.Helper;
using JLib.Reflection.Attributes;
using JLib.ValueTypes;
using static JLib.Reflection.Attributes.TvtFactoryAttributes;

namespace JLib.Reflection;


[IsDerivedFromAny(typeof(ValueType<>))]
public record ValueTypeType(Type Value) : TypeValueType(Value), IValidatedType
{
    public Type NativeType
    {
        get
        {
            try
            {
                return Value.GetAnyBaseType<ValueType<Ignored>>()?.GenericTypeArguments.First()!;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public bool Mapped => !Value.HasCustomAttribute<UnmappedAttribute>() && !Value.IsAbstract;
    void IValidatedType.Validate(ITypeCache cache, TvtValidator value)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (NativeType is null)
            value.AddError("the NativeType could not be found");
    }
}

