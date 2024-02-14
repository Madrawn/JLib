using JLib.ValueTypes;

namespace JLib.DataGeneration.Examples.Setup.Models;

public record CustomerId(Guid Value) : GuidValueType(Value);

public class CustomerEntity
{
    public CustomerEntity(string userName)
    {
        Id = new(Guid.NewGuid());
        UserName = userName;
    }

    public CustomerId Id { get; init; }
    public string UserName { get; set; }
}