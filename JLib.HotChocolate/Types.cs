using System.Reflection;
using JLib.Data;
using JLib.Helper;
using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.HotChocolate;

[ImplementsAny(typeof(IMappedGraphQlDataObject<>)), NotAbstract, IsClass, Priority(GraphQlDataObjectType.NextPriority)]
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
