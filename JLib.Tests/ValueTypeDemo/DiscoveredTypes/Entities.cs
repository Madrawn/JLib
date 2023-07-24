using JLib.Tests.ValueTypeDemo.BaseTypes;

namespace JLib.Tests.ValueTypeDemo.DiscoveredTypes;

public class OrderQe : QueryEntity
{

}

public class OrderCe : CommandEntity, IOrderRoe
{

}

public interface IOrderRoe : IReadOnlyEntity
{

}
