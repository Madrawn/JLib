using JLib.Exceptions;
using JLib.Helper;
using JLib.Tests.ValueTypeDemo.BaseTypes;

using static JLib.FactoryAttributes.TvtFactoryAttributes;

namespace JLib.Tests.ValueTypeDemo.TypeValueTypes;
public partial class TypeValueTypes
{
    [Implements<IEntity>]
    public abstract record Entity(Type Value) : NavigatingTypeValueType(Value)
    {
        public abstract QueryEntity? Query { get; }
        public abstract CommandEntity Command { get; }
        public abstract ReadOnlyEntity ReadOnly { get; }
    }


    [IsDerivedFrom<BaseTypes.QueryEntity>, NotAbstract]
    public record QueryEntity(Type Value) : Entity(Value)
    {
        public override QueryEntity Query => this;

        public override CommandEntity Command => Navigate(cache =>
        {
            var candidates = cache.All<CommandEntity>()
                .Where(mce =>
                    mce.Value
                        .GetInterface<IQueryableEntity<BaseTypes.QueryEntity>>()
                        ?.GenericTypeArguments
                        .First()
                    == Value)
                .ToArray();
            if (candidates.None())
                throw new InvalidSetupException($"no interface found");
            if (candidates.Length > 1)
                throw new InvalidSetupException("multiple interfaces found");

            return candidates.Single();
        });

        public override ReadOnlyEntity ReadOnly => Navigate(cache =>
        {
            var candidates = Value.GetInterfaces()
                .Select(cache.TryGet<ReadOnlyEntity>)
                .WhereNotNull()
                .ToArray();
            if (candidates.None())
                throw new InvalidSetupException($"no interface found");
            if (candidates.Length > 1)
                throw new InvalidSetupException("multiple interfaces found");
            return candidates.Single();
        });
    }

    [IsAssignableTo<BaseTypes.CommandEntity>, NotAbstract]
    public record CommandEntity(Type Value) : Entity(Value)
    {
        private Lazy<QueryEntity?>? _queryEntity;
        public override QueryEntity? Query { get; }
        public override CommandEntity Command { get; }
        public override ReadOnlyEntity ReadOnly { get; }
    }
    [IsAssignableTo<IReadOnlyEntity>, IsInterface]
    public record ReadOnlyEntity(Type Value) : Entity(Value);
}
