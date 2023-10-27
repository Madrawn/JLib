using AutoMapper;
using JLib.Attributes;
using JLib.Data;
using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

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

[IsDerivedFrom(typeof(Profile)), NotAbstract]
public record AutoMapperProfileType(Type Value) : TypeValueType(Value)
{
    private static readonly Type[] CtorParamArray = new[] { typeof(ITypeCache) };
    public Profile Create(ITypeCache typeCache)
        => Value.GetConstructor(Array.Empty<Type>())
            ?.Invoke(null).As<Profile>()
        ?? Value.GetConstructor(CtorParamArray)
            ?.Invoke(new object[] { typeCache })
            .As<Profile>()
        ?? throw new InvalidOperationException($"Instantiation of {Name} failed.");
}

public abstract record DataObjectType(Type Value) : NavigatingTypeValueType(Value), IDataObjectType
{
    public const int NextPriority = 10_000;
}


[Implements(typeof(IEntity)), IsClass, NotAbstract]
public record EntityType(Type Value) : DataObjectType(Value), IValidatedType
{
    public new const int NextPriority = DataObjectType.NextPriority - 1_000;
    public virtual void Validate(ITypeCache cache, TvtValidator value)
    {
        if (GetType() == typeof(EntityType))
            value.AddError($"You have to specify which type of entity this is by implementing a derivation of the {nameof(IEntity)} interface");
    }
}

/// <summary>
/// An entity which uses ValueTypes to ensure data validity
/// </summary>
[Implements(typeof(ICommandEntity)), IsClass, NotAbstract, Priority(EntityType.NextPriority)]
public record CommandEntityType(Type Value) : EntityType(Value)
{
    public new const int NextPriority = EntityType.NextPriority - 1_000;

}
public abstract record DataProviderType(Type Value) : NavigatingTypeValueType(Value), IPostNavigationInitializedType, IValidatedType
{
    public bool CanWrite { get; private set; }

    void IPostNavigationInitializedType.Initialize(ITypeCache _, IExceptionManager exceptions)
    {
        CanWrite = Value.ImplementsAny<IDataProviderRw<IEntity>>();
    }

    public virtual void Validate(ITypeCache cache, TvtValidator value) { }
}

[ImplementsAny(typeof(IDataProviderR<>)), BeGeneric, NotAbstract, IsClass]
public record SourceDataProviderType(Type Value) : DataProviderType(Value)
{
    public override void Validate(ITypeCache cache, TvtValidator value)
    {
        base.Validate(cache, value);
        if (Value.ImplementsAny<IDataProviderRw<IEntity>>())
            value.ShouldImplementAny<ISourceDataProviderRw<IEntity>>();
        else
            value.ShouldImplementAny<ISourceDataProviderR<IDataObject>>();
    }
}

[ImplementsAny(typeof(IDataProviderR<>)), NotBeGeneric, NotAbstract, IsClass]
public record RepositoryType(Type Value) : DataProviderType(Value)
{
    public DataObjectType ProvidedDataObject
        => Navigate(cache => Value.GetAnyInterface<IDataProviderR<IDataObject>>()?.GenericTypeArguments.First()
            .AsValueType<DataObjectType>(cache)!);
    public override void Validate(ITypeCache cache, TvtValidator value)
    {
        base.Validate(cache, value);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ProvidedDataObject is null)
            value.AddError("The Data Object type could not be resolved");
        value.ShouldNotBeGeneric(Environment.NewLine + $"if you tried to build a generic data provider, you have to implement {nameof(ISourceDataProviderR<IDataObject>)} or {nameof(ISourceDataProviderRw<IEntity>)}"
            + Environment.NewLine + $"If you tried to build a Repository, you have to implement {nameof(IDataProviderR<IDataObject>)} or {nameof(IDataProviderRw<IEntity>)}");
        value.ShouldNotImplementAny<ISourceDataProviderR<IDataObject>>();
    }
}