using Microsoft.Extensions.DependencyInjection;

namespace JLib.Helper;

public abstract record ServiceContainer
{
    internal ServiceContainer() { }
    internal virtual void Init(IServiceProvider provider) { }
}
public record ServiceContainer<T1> : ServiceContainer
where T1 : notnull
{
    internal override void Init(IServiceProvider provider)
    {
        base.Init(provider);
        this.Service1 = provider.GetRequiredService<T1>();
    }

    public T1 Service1 { get; private set; } = default!;

    public void Deconstruct(out T1 service1)
        => service1 = this.Service1;
}
public record ServiceContainer<T1, T2> : ServiceContainer<T1>
where T1 : notnull
where T2 : notnull
{
    internal override void Init(IServiceProvider provider)
    {
        base.Init(provider);
        this.Service2 = provider.GetRequiredService<T2>();
    }

    public T2 Service2 { get; private set; } = default!;

    public void Deconstruct(out T1 service1, out T2 service2)
    {
        service1 = this.Service1;
        service2 = this.Service2;
    }
}
public record ServiceContainer<T1, T2, T3> : ServiceContainer<T1, T2>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
{
    internal override void Init(IServiceProvider provider)
    {
        base.Init(provider);
        this.Service3 = provider.GetRequiredService<T3>();
    }

    public T3 Service3 { get; private set; } = default!;

    public void Deconstruct(out T1 service1, out T2 service2, out T3 service3)
    {
        service1 = this.Service1;
        service2 = this.Service2;
        service3 = this.Service3;
    }
}
public record ServiceContainer<T1, T2, T3, T4> : ServiceContainer<T1, T2, T3>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
{
    internal override void Init(IServiceProvider provider)
    {
        base.Init(provider);
        this.Service4 = provider.GetRequiredService<T4>();
    }

    public T4 Service4 { get; private set; } = default!;

    public void Deconstruct(out T1 service1, out T2 service2, out T3 service3, out T4 service4)
    {
        service1 = this.Service1;
        service2 = this.Service2;
        service3 = this.Service3;
        service4 = this.Service4;
    }
}
public record ServiceContainer<T1, T2, T3, T4, T5> : ServiceContainer<T1, T2, T3, T4>
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
    where T4 : notnull
    where T5 : notnull
{
    internal override void Init(IServiceProvider provider)
    {
        base.Init(provider);
        this.Service5 = provider.GetRequiredService<T5>();
    }

    public T5 Service5 { get; private set; } = default!;

    public void Deconstruct(out T1 service1, out T2 service2, out T3 service3, out T4 service4, out T5 service5)
    {
        service1 = this.Service1;
        service2 = this.Service2;
        service3 = this.Service3;
        service4 = this.Service4;
        service5 = this.Service5;
    }
}