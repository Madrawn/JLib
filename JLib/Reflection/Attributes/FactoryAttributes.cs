using JLib.Helper;

namespace JLib.Reflection.Attributes;

/// <summary>
/// classes with this given attribute will not be ignored by the typeCache
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IgnoreInCache : Attribute
{
}

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
    public sealed class PriorityAttribute : Attribute
    {
        public int Value { get; }

        public PriorityAttribute(int value)
        {
            Value = value;
        }

        public const int DefaultPriority = 10_000;
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class IsInterfaceAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.IsInterface;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IsClassAttribute : Attribute, ITypeValueTypeFilterAttribute
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
    public class NotAbstractAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.IsAbstract;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BeGenericAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.IsGenericType;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NotBeGenericAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.IsGenericType;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAnyAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public IsDerivedFromAnyAttribute(Type type)
        {
            _type = type;
        }

        public bool Filter(Type type)
            => type.IsDerivedFromAny(_type);
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsAssignableToAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public IsAssignableToAttribute(Type type)
        {
            _type = type;
        }

        public bool Filter(Type type)
            => type.IsAssignableTo(_type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public IsDerivedFromAttribute(Type type)
        {
            _type = type;
        }

        public bool Filter(Type type)
            => type.IsAssignableTo(_type) && type != _type;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public ImplementsAttribute(Type type)
        {
            _type = type;
        }

        public bool Filter(Type type)
            => type.Implements(_type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsAnyAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public ImplementsAnyAttribute(Type type)
        {
            _type = type;
        }

        public bool Filter(Type type)
            => type.ImplementsAny(_type);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ImplementsNoneAttribute : Attribute, ITypeValueTypeFilterAttribute
    {
        private readonly Type _type;

        public ImplementsNoneAttribute(Type type)
        {
            _type = type;
        }

        public bool Filter(Type type)
            => !type.ImplementsAny(_type);
    }
#if NET7_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsAssignableToAttribute<T> : Attribute, ITypeValueTypeFilterAttribute
        where T : class
    {
        public bool Filter(Type type)
            => type.IsAssignableTo<T>();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAttribute<T> : Attribute, ITypeValueTypeFilterAttribute
        where T : class
    {
        public bool Filter(Type type)
            => type.IsAssignableTo<T>() && type != typeof(T);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IsDerivedFromAnyAttribute<T> : Attribute, ITypeValueTypeFilterAttribute
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
    public class ImplementsAttribute<T> : Attribute, ITypeValueTypeFilterAttribute
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
    public class ImplementsAnyAttribute<T> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => type.ImplementsAny<T>();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsNoneAttribute<T> : Attribute, ITypeValueTypeFilterAttribute
    {
        public bool Filter(Type type)
            => !type.ImplementsAny<T>();
    }
#endif
}