using System.Reflection;
using AutoMapper;
using JLib.Attributes;
using JLib.Data;
using JLib.FactoryAttributes;
using JLib.Helper;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.VisualBasic.CompilerServices;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib;

/// <summary>
/// used for entity mapping in conjunction with any <see cref="IMappedDataObjectType"/>.
/// <br/>Enables the Profile to ignore the prefix when resolving the correlated properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PropertyPrefixAttribute : Attribute
{
    public string Prefix { get; }

    public PropertyPrefixAttribute(string prefix)
    {
        Prefix = prefix;
    }
}

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

[ImplementsAny(typeof(IMappedGraphQlDataObject<>)), NotAbstract, IsClass, Priority(3_000)]
public sealed record MappedGraphQlDataObjectType(Type Value) : GraphQlDataObjectType(Value), IMappedDataObjectType, IPostNavigationInitializedType
{
    public MappedCommandEntityType? CommandEntity
        => Navigate(typeCache => typeCache.TryGet<MappedCommandEntityType>(cmd => cmd.SourceEntity == SourceEntity));
    public EntityType SourceEntity
        => Navigate(typeCache => Value.GetAnyInterface<IMappedGraphQlDataObject<IEntity>>()?.GenericTypeArguments.First()
                                  .CastValueType<EntityType>(typeCache)
                              ?? throw NewInvalidTypeException("SourceEntity could not be found"));
    /// <summary>
    /// add the <see cref="PropertyPrefixAttribute"/> to the <see cref="SourceEntity"/> type
    /// </summary>//
    public PropertyPrefix? PropertyPrefix { get; private set; }

    public bool ReverseMap => false;

    public void Initialize(IExceptionManager exceptions)
        => PropertyPrefix = SourceEntity.Value.GetCustomAttribute<PropertyPrefixAttribute>()?.Prefix;
}

[ImplementsAny(typeof(IMappedCommandEntity<>)), NotAbstract, Priority(3_000)]
public record MappedCommandEntityType(Type Value) : CommandEntityType(Value), IMappedDataObjectType, IPostNavigationInitializedType
{
    public EntityType SourceEntity =>
        Navigate(typeCache => Value.GetAnyInterface<IMappedCommandEntity<IEntity>>()?.GenericTypeArguments.First()
                                  .CastValueType<EntityType>(typeCache)
                              ?? throw NewInvalidTypeException("SourceEntity could not be found"));
    public PropertyPrefix? PropertyPrefix { get; private set; }
    public virtual bool ReverseMap => true;
    void IPostNavigationInitializedType.Initialize(IExceptionManager exceptions)
        => PropertyPrefix = SourceEntity.Value.GetCustomAttribute<PropertyPrefixAttribute>()?.Prefix;
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
}


[Implements(typeof(IEntity)), IsClass, NotAbstract]
public record EntityType(Type Value) : DataObjectType(Value), IValidatedType
{
    public virtual void Validate(ITypeCache cache, TvtValidator value)
    {
        if (GetType() == typeof(EntityType))
            value.AddError($"You have to specify which type of entity this is by implementing a derivation of the {nameof(IEntity)} interface");
    }
}

/// <summary>
/// An entity which uses ValueTypes to ensure data validity
/// </summary>
[Implements(typeof(ICommandEntity)), IsClass, NotAbstract, Priority(5_000)]
public record CommandEntityType(Type Value) : EntityType(Value)
{

}
[Implements(typeof(IGraphQlDataObject)), IsClass, NotAbstract]
public record GraphQlDataObjectType(Type Value) : DataObjectType(Value), IValidatedType
{
    public void Validate(ITypeCache cache, TvtValidator value)
    {
        var ctors = Value.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (ctors.Length == 1)
        {
            var ctor = ctors.Single();
            if (ctor.GetParameters().Any())
                value.AddError("parameters found on the only constructor. A parameterless cosntructor is required");
        }
        else
        {
            var propsToInitialize = Value.GetProperties().Any(prop =>
                prop.CanWrite && !prop.IsNullable() && !prop.PropertyType.ImplementsAny<IEnumerable<Ignored>>());
            var hasPublicParameterlessCtor = ctors.Any(ctor => ctor.GetParameters().None() && ctor.IsPublic);
            if (propsToInitialize && hasPublicParameterlessCtor)
                value.AddError("found a public parameterless ctor despite having non-nullable properties");
        }
    }
}

public abstract record DataProviderType(Type Value) : NavigatingTypeValueType(Value), IPostNavigationInitializedType, IValidatedType
{
    public bool CanWrite { get; private set; }

    void IPostNavigationInitializedType.Initialize(IExceptionManager exceptions)
    {
        CanWrite = Value.ImplementsAny<IDataProviderRw<IEntity>>();
    }

    public virtual void Validate(ITypeCache cache, TvtValidator value) { }
}

[ImplementsAny(typeof(IDataProviderR<>)), BeGeneric, NotAbstract, IsClass, Priority(7000)]
public record SourceDataProviderType(Type Value) : DataProviderType(Value), IValidatedType
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

[ImplementsAny(typeof(IDataProviderR<>)), NotBeGeneric, NotAbstract, IsClass, Priority(7000)]
public record RepositoryType(Type Value) : DataProviderType(Value), IValidatedType
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