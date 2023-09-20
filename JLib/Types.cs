using System.Reflection;
using AutoMapper;
using JLib.Attributes;
using JLib.Data;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib;
public static class Types
{
    [IsDerivedFromAny(typeof(ValueType<>))]
    public record ValueType(Type Value) : TypeValueType(Value), IValidatedType
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
        void IValidatedType.Validate(ITypeCache cache, TvtValidator exceptions)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (NativeType is null)
                exceptions.Add("the NativeType could not be found");
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

    public abstract record DataObject(Type Value) : NavigatingTypeValueType(Value)
    {
    }


    [Implements(typeof(IEntity)), IsClass, NotAbstract]
    public record EntityType(Type Value) : DataObject(Value), IValidatedType
    {
        public GraphQlDataObject? GraphQlDataObject =>
            Navigate(cache => cache.All<GraphQlDataObject>(gdo => gdo.CommandEntity == this).SingleOrDefault());
        public void Validate(ITypeCache cache, TvtValidator validator)
        {
            if (GetType() == typeof(EntityType))
                validator.Add($"You have to specify which type of entity this is by implementing a derivation of the {nameof(IEntity)} interface");
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
    public record GraphQlDataObject(Type Value) : DataObject(Value), IValidatedType
    {
        public EntityType? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlDataObject<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<EntityType>(cache));

        public void Validate(ITypeCache cache, TvtValidator validator)
        {
            var ctors = Value.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (ctors.Length == 1)
            {
                var ctor = ctors.Single();
                if (ctor.GetParameters().Any())
                    validator.Add("parameters found on the only constructor. A parameterless cosntructor is required");
            }
            else
            {
                var propsToInitialize = Value.GetProperties().Any(prop =>
                    prop.CanWrite && !prop.IsNullable() && !prop.PropertyType.ImplementsAny<IEnumerable<Ignored>>());
                var hasPublicParameterlessCtor = ctors.Any(ctor => ctor.GetParameters().None() && ctor.IsPublic);
                if (propsToInitialize && hasPublicParameterlessCtor)
                    validator.Add("found a public parameterless ctor despite having non-nullable properties");
            }
        }
    }
    [ImplementsAny(typeof(IGraphQlMutationParameter<>)), IsClass, NotAbstract]
    public record GraphQlMutationParameter(Type Value) : DataObject(Value)
    {
        public EntityType? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlMutationParameter<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<EntityType>(cache));
    }
}
