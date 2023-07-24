namespace JLib.FactoryAttributes;

public abstract class TvtFactoryAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DerivesFrom<T> : Attribute
    where T : class
    { }
    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementationOfInterface<T> : Attribute { }
    [AttributeUsage(AttributeTargets.Class)]
    public class IsInterface : Attribute { }
    [AttributeUsage(AttributeTargets.Class)]
    public class IsClass : Attribute { }
    [AttributeUsage(AttributeTargets.Class)]
    public class NotAbstract : Attribute { }
    [AttributeUsage(AttributeTargets.Class)]
    public class Implements<T> : Attribute { }
    [AttributeUsage(AttributeTargets.Class)]
    public class ImplementsAny<T> : Attribute { }
}
