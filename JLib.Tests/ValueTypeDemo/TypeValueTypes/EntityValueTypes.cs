using JLib.Exceptions;
using JLib.Helper;
using JLib.Tests.ValueTypeDemo.BaseTypes;

using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Tests.ValueTypeDemo.TypeValueTypes;
public partial class TypeValueTypes
{
    [Implements<IEntity>]
    public abstract record Entity(Type Value) : NavigatingTypeValueType(Value), IPostInitValidatedType
    {
        public abstract QueryEntity? Query { get; }
        public abstract CommandEntity Command { get; }
        public abstract ReadOnlyEntity ReadOnly { get; }

        public void PostInitValidation(ITypeCache cache)
        {
            if (Command is null)
                throw new InvalidTypeException(nameof(Command) + "is null");
            if (ReadOnly is null)
                throw new InvalidTypeException(nameof(ReadOnly) + "is null");
        }
    }


    [IsAssignableTo<BaseTypes.QueryEntity>, NotAbstract]
    public record QueryEntity(Type Value) : Entity(Value)
    {
        public override QueryEntity Query => this;

        public override CommandEntity Command => Navigate(cache =>
        {
            var candidates = cache.All<CommandEntity>()
                .Where(mce =>
                    mce.Value
                        .GetAnyInterface<IQueryableEntity<BaseTypes.QueryEntity>>()
                        ?.GenericTypeArguments
                        .First()
                    == Value)
                .ToArray();
            if (candidates.None())
                throw TvtNavigationFailedException.Create<QueryEntity, CommandEntity>(Value,
                    "no interface found");
            if (candidates.Length > 1)
                throw TvtNavigationFailedException.Create<QueryEntity, CommandEntity>(Value,
                    "multiple interfaces found");

            return candidates.Single();
        });

        public override ReadOnlyEntity ReadOnly => Command.ReadOnly;
    }

    [IsAssignableTo<BaseTypes.CommandEntity>, NotAbstract]
    public record CommandEntity(Type Value) : Entity(Value)
    {
        public override QueryEntity? Query => Navigate(cache
            => Value.GetAnyInterface<IQueryableEntity<BaseTypes.QueryEntity>>() is { } i
            ? cache.Get<QueryEntity>(i.GenericTypeArguments.First())
            : null);

        public override CommandEntity Command => this;

        public override ReadOnlyEntity ReadOnly => Navigate(cache
            => Value.GetInterfaces().Select(cache.TryGet<ReadOnlyEntity>).WhereNotNull().Single()
        );
    }

    [IsAssignableTo<IReadOnlyEntity>, IsInterface]
    public record ReadOnlyEntity(Type Value) : Entity(Value)
    {
        public override QueryEntity? Query => Command.Query;
        public override CommandEntity Command => Navigate(cache =>
            cache.All<CommandEntity>().SingleOrDefault(ce => ce.ReadOnly == this)
            ?? throw TvtNavigationFailedException.Create<ReadOnlyEntity, CommandEntity>(Value,
                "no command entities found"));
        public override ReadOnlyEntity ReadOnly => this;
    }
}
