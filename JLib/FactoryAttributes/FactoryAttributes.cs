using JLib.Helper;

namespace JLib.FactoryAttributes;

/// <summary>
/// classes with this given attribute will not be ignored by the typeCache
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreInCache : Attribute { }
public abstract class TvtFactoryAttributes
{
    public interface ITypeValueTypeFilterAttribute
    {
        bool Filter(Type type);
    }
    /// <summary>
    /// lowest wins
    /// <br/>default is 10_000
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Priority : Attribute
    {
        public int Value { get; }

        public Priority(int value)
        {
            Value = value;
        }

        public const int DefaultPriority = 10_000;
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
    public class HasAttributeAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        public HasAttributeAttribute(Type attributeType)
        {
            AttributeType = attributeType;
        }

        public Type AttributeType { get; }
        public bool Filter(Type type)
            => type.HasCustomAttribute(AttributeType);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotAbstract : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.IsAbstract;
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class BeGeneric : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.IsGenericType;
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class NotBeGeneric : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.IsGenericType;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAny : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public IsDerivedFromAny(Type type)
        {
            _type = type;
        }
        public bool Filter(Type type)
            => type.IsDerivedFromAny(_type);
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFrom : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public IsDerivedFrom(Type type)
        {
            _type = type;
        }
        public bool Filter(Type type)
            => type.IsAssignableTo(_type) && type != _type;
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class Implements : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public Implements(Type type)
        {
            _type = type;
        }
        public bool Filter(Type type)
            => type.Implements(_type);
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAny : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public ImplementsAny(Type type)
        {
            _type = type;
        }
        public bool Filter(Type type)
            => type.ImplementsAny(_type);
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsNone : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public ImplementsNone(Type type)
        {
            _type = type;
        }
        public bool Filter(Type type)
            => !type.ImplementsAny(_type);
    }
#if NET7_0_OR_GREATER

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
#endif
}
