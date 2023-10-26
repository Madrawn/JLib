## Requirements: 
- [AuthorizationExtensions](./AuthorizationExtensions.cs).AddDataAuthorization
- ServiceCollectionHelper.AddScopes

# Adding Authorization Profiles
1. create a new class
1. inherit from [AuthorizationProfile](AuthorizationProfile.cs)
1. call the AddAuthorization method in you constructor
1. make sure that you have only one authprization rule per dataObject class

# Adding Authorization to a DataProvider
1. Inject [IAuthorizationInfo\<TDo>](AuthorizationInfo.cs)
2. Apply the Authorization
   - Invoke the AuthorizeQuery method and pass the resulting Expression to a Where Clause of your queryable
   - Invoke the AuthorizeDataObject method to check whether a given object is authorized
   - invoke the RaiseExceptionIfUnauthorised method to raise an exception if the given object is unauthorised. It returns the given object enable in-line calls

<br/>note: the MockDataProvider contains an example implementation of authorization
   


## How it works
- the manager reads all profiles
- then it creates a map from the entity to the authInfo contained in the profile
- when the profile is accessed, a scopedAuthInfo is created to make the check scope relative.
- the scopedAuthInfo resolves on authorization the requested services and forwards them to the configured methods

## How to add more AuthorizationProfile.AddAuthorization Overloads:
- you either have to add one overload and
    - one class per new type argument or:
    - make the auth funcs type arguments or:
    - use the ServiceContainer. this would add boilerplate to the authentication call