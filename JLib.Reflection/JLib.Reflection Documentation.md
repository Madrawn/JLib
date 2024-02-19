# Jlib.Reflection
The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL
      NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED",  "MAY", and
      "OPTIONAL" in this document are to be interpreted as described in
      [RFC 2119](https://www.ietf.org/rfc/rfc2119.txt).
## Features
- compile time Typesafety: a function may opnly accept EntityTypes instead of all types
- improved Readabillity through communication of content: you do not get a Type but an EntityType or and DtoType
- reduced Validation: you do not have to check if the type is valid each time you want to use it
- navigation properties: a TypeValueType might contain References to other TypevalueTypes, such as EntityType.MappedDataTransferObject
- utility methods: you are able to create methods to simplify the usage, for example to automatically add type arguments to the type
- start-time validation: on startup, each valuetype can be validated and the solution won't run if an type is invalid, for example missing a required interface
- reflection-caching: All reflections are executed once and use from that point the calculated result.
- simplified type discovery: you can use the TvtFactoryAttributes to automatically discover all your types
- DRY-er code: you define your reflections at one central location per TypeValueType, which prevents you from having to write the same code multiple times
- simplified reflection: you no longer have to think about what defines an entity when trying to build something like a AutoMapper.Profile using reflection, since it has already been done.
### Components
- TypeValueType
    - Combines the [DDD ValueObjects / Value Types](https://blog.jannikwempe.com/domain-driven-design-entities-value-objects) with the System.Type
    - see: Jlib.Reflection.TypeValueType
    - MUST NOT be instantiated manually but by using the TypeCache
    - MUST be decorated using FactoryAttributes
    - Types MUST only be assignable to a single TypeValueType according to the TypeValueTypes FactoryAttributes
- FactoryAttributes
    - provides a simplified way to define which types are, for example, Entities simply by adding attributes.
    - see: JLib.Reflection.TvtFactoryAttributes
    - MUST implement JLib.Reflection.TvtFactoryAttributes.ITypeValueTypeFilterAttribute
    - MUST only be used on classes derived from TypeValueType
- TypeCache
    - Created when the application starts. Handles the Initialization and delivery of TypeValueTypes
    - see: TypeCache, JLib.ServiceCollectionHelper.AddTypeCache
    - SHOULD: be used as singleton
- TypePackages
    - 


Improvements
- Consolidate (Dsiable-) Automapper Pfofile Attributes
- Improve JLib.Automapper Separation from JLib.Reflection - a lot of separation of concern issues are not addressed (i.E. the HasCustomAutoMapperProfile Property of TypeValueType)