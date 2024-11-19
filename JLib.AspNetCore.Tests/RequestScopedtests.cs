using FluentAssertions;
using JLib.DependencyInjection;
using JLib.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace JLib.AspNetCore.Tests;

public class RequestScopedTests : IDisposable
{
    #region test services
    public class ThrowOnCreate
    {
        public class Exception : System.Exception { }
        public ThrowOnCreate()
        {
            throw new Exception();
        }
    }
    public class ReferenceOther<T>
    {
        public ReferenceOther(T other)
        {

        }
    }
    public class UnsupportedGeneric<T>
    {
        public UnsupportedGeneric(T other)
        {

        }
    }

    public class NotProvided { }
    public class Identifiyable
    {
        public Guid ServiceId { get; } = Guid.NewGuid();
    }
    public class TestHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = null;
    }
    #endregion
    private readonly List<IDisposable> _disposables = new();
    public void Dispose() => _disposables.DisposeAll();
    private readonly ServiceProvider _provider;

    public RequestScopedTests()
    {
        var services = new ServiceCollection();
        services.AddScoped<TestHttpContextAccessor>();
        services.AddScopedAlias<IHttpContextAccessor, TestHttpContextAccessor>();
        services.AddRequestScoped(typeof(ReferenceOther<UnsupportedGeneric<object>>));
        services.AddRequestScoped(typeof(ReferenceOther<NotProvided>));
        services.AddRequestScoped<ThrowOnCreate>();
        services.AddRequestScoped<Identifiyable>();

        _provider = services.BuildServiceProvider()
            .DisposeWith(_disposables);
    }

    private void CreateRequestScope(out HttpContext httpContext)
        => CreateRequestScope(out httpContext, out _);
    private void CreateRequestScope(out HttpContext httpContext, out IServiceProvider serviceProvider)
    {
        var scope = _provider.CreateScope().DisposeWith(_disposables);
        serviceProvider = scope.ServiceProvider;
        httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        serviceProvider.GetRequiredService<TestHttpContextAccessor>().HttpContext = httpContext;
    }
    private void CreateSubScope(HttpContext httpContext, out IServiceProvider serviceProvider)
    {
        var scope = _provider.CreateScope().DisposeWith(_disposables);
        serviceProvider = scope.ServiceProvider;
        serviceProvider.GetRequiredService<TestHttpContextAccessor>().HttpContext = httpContext;
    }

    [Fact]
    public void ServiceCombinationAndSeparationWorks()
    {
        CreateRequestScope(out var contextA, out var providerA);
        CreateSubScope(contextA, out var providerA2);
        CreateRequestScope(out _, out var providerB);

        providerA.GetRequiredService<Identifiyable>().ServiceId
            .Should().Be(providerA2.GetRequiredService<Identifiyable>().ServiceId)
            .And.NotBe(providerB.GetRequiredService<Identifiyable>().ServiceId);
    }
    [Fact]
    public void ThrowingService()
    {
        CreateRequestScope(out var contextA, out var providerA);
        CreateSubScope(contextA, out var providerA2);
        CreateRequestScope(out _, out var providerB);

        Action act = () => providerA2.GetRequiredService<ThrowOnCreate>();
        act.Should()
            .Throw<ThrowOnCreate.Exception>();
    }
    [Fact]
    public void FailingMainService()
    {
        CreateRequestScope(out var contextA, out var providerA);
        CreateSubScope(contextA, out var providerA2);

        Action act = () => providerA2.GetRequiredService<ReferenceOther<UnsupportedGeneric<object>>>();
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("No service for type '*' has been registered.");
    }
    [Fact]
    public void MissingReferencedService()
    {
        CreateRequestScope(out var contextA, out var providerA);
        CreateSubScope(contextA, out var providerA2);

        Action act = () => providerA.GetRequiredService<ReferenceOther<NotProvided>>();
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("No service for type '*' has been registered.");
    }
    [Fact]
    public void MissingHttpContext()
    {

        Action act = () => _provider.GetRequiredService<Identifiyable>();
        act.Should()
            .Throw<AspNetCoreServiceCollectionExtensions.AddRequestScopedServiceException.OutsideHttpContextScopeException>();
    }
}