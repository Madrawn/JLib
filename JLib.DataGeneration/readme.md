
# Project Description
This library allows you to manage your testing data using packages and simplifies the use of Ids.

## The Concept
### Persistent IDs
A Data Package can define public Id-Properties which will be automatically set to the same id each time the test is run.
The Id can be either a int, Guid, or a derivative of IntValueType or GuidValueType.
The int and Guid Ids are persisted in a file stored next to the project file, containing all ids for the entire project.
there are two variants of ids: property and named ids.
#### Property IDs
A property id is defined by adding a property of one of the types noted above to the data package the getter should be public and the must be present setter and protected.
To get the property id of another package, inject the package and access the property.
#### Named IDs
A named id is defined by getting it from the datapackage base class. the name must be unique per dataPackage.
NamedIDs should be used when it is not neccessary to reference this entity from another entity like when creating a n:m reference entity.
The name of the id should contain the propertynames of the related ids (for example when you create orderItems for the order named 'CompletedOrder', the name should be 'CompletedOrder_Item_1')
A named Id can not be accessed from another package. 

### Data Packages
#### Extentions
#### References





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