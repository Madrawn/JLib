using System.Reflection;
using JLib.Helper;

namespace JLib.FactoryAttributes;

public abstract class TvtFactoryAttributes
{
    public interface ITypeValueTypeFilterAttribute
    {
        bool Filter(Type type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsAssignableTo<T> : Attribute, ITypeValueTypeFilterAttribute
        where T : class
    {
        public bool Filter(Type type)
            => type.IsAssignableTo<T>();
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFrom<T> : Attribute, ITypeValueTypeFilterAttribute
        where T : class
    {
        public bool Filter(Type type)
            => type.IsAssignableTo<T>() && type != typeof(T);
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAny<T> : Attribute, ITypeValueTypeFilterAttribute
        where T : class
    {
        public bool Filter(Type type)
            => type.IsDerivedFromAny<T>();
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsNotDerivedFromAny<T> : Attribute, ITypeValueTypeFilterAttribute
        where T : class
    {
        public bool Filter(Type type)
            => !type.IsDerivedFromAny<T>();
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsNotThisTvtAttribute<TTvt> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => typeof(TTvt).GetCustomAttributes().OfType<ITypeValueTypeFilterAttribute>().All(a => a.Filter(type)) is false;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Implements<T> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.Implements<T>();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsNot<T> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.Implements<T>();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsAny<T> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.ImplementsAny<T>();
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsNone<T> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.ImplementsAny<T>();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsInterface : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.IsInterface;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsClass : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.IsClass;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsNotAbstract : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.IsAbstract;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotAbstract : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.IsAbstract;
    }
}
