# JLib.ValueTypes Architecture Decisions
This document contains the options and final decisions of this project.

# Validation
## Validator passed to the ctor
```cs
public record DemoAlphanumericString(string Value, value=>value.BeAlphanumeric()) : DemoAsciiString(Value);
```
### Pros
- Unambiguous prototype
- ease of use
- primary ctor support
### Cons
- if a base type does not implement the validator, the derived class can not use it
- the caller might pass additional validators when creating the object
- possibly unneccessary boilerplate code
## Static validation property
```cs
public record DemoAlphanumericString(string Value, value=>value.BeAlphanumeric()) : DemoAsciiString(Value)
{
    [ValueTypeValidator]
    private static readonly StringValidationProfile<DemoAlphanumericString> Validator
        = new(v => v.BeAlphanumeric());
}
```
## Non staic method with override  
```cs
public record DemoAlphanumericString(string Value, value=>value.BeAlphanumeric()) : StringValueType(Value)
{
    protected override void Validate(ValidationContext<string?> validator)
    {
        validator.BeAlphanumeric();
        base.
    }
}
```