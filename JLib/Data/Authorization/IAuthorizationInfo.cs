using JLib.Reflection;

namespace JLib.Data.Authorization;

public interface IAuthorizationInfo
{
    DataObjectType Target { get; }
}
