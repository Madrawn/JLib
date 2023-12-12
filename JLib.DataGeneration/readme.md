
# Project Description
Package based data generator with deterministic id resolution

## How to

### Create a class Derived from DataPackage and implement the constructor
```cs
using JLib.DataGeneration;
// ...
public class MyDataPackage : DataPackage
{
    public MyDataPackage(IDataPackageStore dataPackages) : base(dataPackages)
    {
    }
}
```
### Get Deterministic Ids
To create deterministic ids, add a public property of type `guid` or a derivative of `GuidValueType` to your `DataPackage`
```cs
using JLib.DataGeneration;
using JLib.ValueTypes;
// ...
public record MyTypedId(Guid Value) : GuidValueType(Value);
public class MyDataPackage : DataPackage
{
    public Guid MyFirstNativeId { get; set; }
    public MyTypedId MyTypedId { get; set; } = default!;
    // ...
}
```

## How it works