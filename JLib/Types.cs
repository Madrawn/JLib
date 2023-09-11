using System;
using System.Collections.Generic;
using System.Linq;
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

    [ImplementsAny<IDataProviderR<Ignored>>, IsClass, IsNotAbstract]
    public record DataProviderImplementation(Type Value) : TypeValueType(Value), IValidatedType
    {
        public bool WriteSupported => Value.ImplementsAny<IDataProviderRw<IEntity>>();
        public Type CreateImplementation(TypeValueType entity, params Func<TypeValueType, TypeValueType>[] typeArgumentResolver)
            => Value.MakeGenericType(typeArgumentResolver.Select(resolver => resolver(this).Value).ToArray());

        void IValidatedType.Validate(ITypeCache cache, TvtValidator validator)
        {
            validator.MustBeGeneric();
        }
    }
    public abstract record DataObject(Type Value) : NavigatingTypeValueType(Value)
    {
    }


    [Implements<IEntity>, IsClass, NotAbstract]
    public record Entity(Type Value) : DataObject(Value)
    {
    }

    [Implements<IGraphQlDataObject>, IsClass, NotAbstract]
    public record GraphQlDataObject(Type Value) : DataObject(Value)
    {
        public Entity? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlDataObject<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<Entity>(cache));
    }
    [Implements<IGraphQlMutationParameter>, IsClass, NotAbstract]
    public record GraphQlMutationParameter(Type Value) : DataObject(Value)
    {
        public Entity? CommandEntity => Navigate(cache =>
            Value.GetAnyInterface<IGraphQlMutationParameter<IEntity>>()
                ?.GenericTypeArguments
                .First()
                .CastValueType<Entity>(cache));
    }
}
