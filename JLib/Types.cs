using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using JLib.Attributes;
using JLib.Data;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
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
    public record EntityType(Type Value) : DataObject(Value)
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
    [Implements<IGraphQlMutationParameter>, IsClass, NotAbstract]
    public record GraphQlMutationParameter(Type Value) : DataObject(Value)
    {
        public EntityType? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlMutationParameter<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<EntityType>(cache));
    }
}
