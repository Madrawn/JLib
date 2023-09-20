using AutoMapper;
using JLib.Attributes;
using JLib.Data;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib;
public static class Types
{
    [IsDerivedFromAny<ValueType<Ignored>>]
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

    [IsDerivedFrom<Profile>, NotAbstract]
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


    [Implements<IEntity>, IsClass, NotAbstract]
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
    [Implements<ICommandEntity>, IsClass, NotAbstract, Priority(5_000)]
    public record CommandEntityType(Type Value) : EntityType(Value)
    {

    }
    [Implements<IGraphQlDataObject>, IsClass, NotAbstract]
    public record GraphQlDataObject(Type Value) : DataObject(Value)
    {
        public EntityType? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlDataObject<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<EntityType>(cache));
    }
    [ImplementsAny<IGraphQlMutationParameter<IEntity>>, IsClass, NotAbstract]
    public record GraphQlMutationParameter(Type Value) : DataObject(Value)
    {
        public EntityType? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlMutationParameter<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<EntityType>(cache));
    }
}
