using FluentAssertions;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ClassNeverInstantiated.Local

namespace JLib.DependencyInjection.Tests;
public class AttributeServicesTest : IDisposable
{
    #region setup
    private readonly ITestOutputHelper _output;

    public AttributeServicesTest(ITestOutputHelper output)
    {
        _output = output;
        _loggerFactory = new LoggerFactory()
            .AddXunit(output);
        _disposables.Add(_loggerFactory);
    }
    private readonly List<IDisposable> _disposables = new();
    private readonly ILoggerFactory _loggerFactory;

    private IServiceProvider CreateSetup<T>(out IExceptionProvider exceptionProvider)
        => CreateSetup<T>(out exceptionProvider, out _);
    private IServiceProvider CreateSetup<T>(out IExceptionProvider exceptionProvider, out IServiceCollection services)
    {
        var exceptions = ExceptionBuilder.Create("test");
        var typePackage = TypePackage.GetNested<T>();
        _output.WriteLine(typePackage.ToString(true));
        services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, _loggerFactory, typePackage)
            .AddServicesWithAttributes(typeCache, exceptions);
        exceptionProvider = exceptions;
        return services.BuildServiceProvider().DisposeWith(_disposables);
    }

    public void Dispose()
    {
        _disposables.DisposeAll();
    }
    #endregion


    private class MinimalClasses
    {
        [Service(ServiceLifetime.Singleton)]
        public class ShoppingService { }
    }
    [Fact]
    public void Minimal()
    {
        CreateSetup<MinimalClasses>(out var ex, out var services)
            .GetRequiredService<MinimalClasses.ShoppingService>()
            .Should().BeOfType<MinimalClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(MinimalClasses.ShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        ex.GetException().Should().BeNull();
    }



    private class AttributeMissingOnServiceClasses
    {
        // provided as singleton
        [Service(ServiceLifetime.Singleton)]
        public class ShoppingService : IShoppingService { }
        // ignored
        public interface IShoppingService { }
    }
    [Fact]
    public void AttributeMissingOnService()
    {
        var provider = CreateSetup<AttributeMissingOnServiceClasses>(out var ex, out var services);
        provider.GetRequiredService<AttributeMissingOnServiceClasses.ShoppingService>()
                    .Should().BeOfType<AttributeMissingOnServiceClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(AttributeMissingOnServiceClasses.ShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        Action invalidAction = () => provider.GetRequiredService<AttributeMissingOnServiceClasses.IShoppingService>();
        invalidAction.Should().Throw<InvalidOperationException>();

        ex.GetException().Should().BeNull();
    }



    private class InjectWithInterfaceClasses
    {
        // not provided
        public class ShoppingService : IShoppingService { }
        // provided as singleton with ShoppingService as implementation (type)
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingService { }
    }
    [Fact]
    public void InjectWithInterface()
    {
        var provider = CreateSetup<InjectWithInterfaceClasses>(out var ex, out var services);

        provider.GetRequiredService<InjectWithInterfaceClasses.IShoppingService>()
            .Should().BeOfType<InjectWithInterfaceClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(InjectWithInterfaceClasses.IShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        Action invalidAction = () => provider.GetRequiredService<InjectWithInterfaceClasses.ShoppingService>();
        invalidAction.Should().Throw<InvalidOperationException>();

        ex.GetException().Should().BeNull();
    }



    private class AttributeOnClassAndInterfaceClasses
    {
        // provided as singleton
        [Service(ServiceLifetime.Singleton)]
        public class ShoppingService : IShoppingService { }
        // provided as singleton with ShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingService { }
    }
    [Fact]
    public void AttributeOnClassAndInterface()
    {
        var provider = CreateSetup<AttributeOnClassAndInterfaceClasses>(out var ex, out var services);

        ex.GetException().Should().BeNull();

        var implementation = provider.GetRequiredService<AttributeOnClassAndInterfaceClasses.ShoppingService>();
        implementation.Should().BeOfType<AttributeOnClassAndInterfaceClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(AttributeOnClassAndInterfaceClasses.ShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        var @interface = provider.GetRequiredService<AttributeOnClassAndInterfaceClasses.IShoppingService>();
        @interface.Should().BeOfType<AttributeOnClassAndInterfaceClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(AttributeOnClassAndInterfaceClasses.IShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        @interface.Should().BeSameAs(implementation);

    }



    private class ImplementationScopeMoreRestrictedThanServiceScopeClasses
    {
        // throws exception, since the implementation has a lower lifetime than the interface
        [Service(ServiceLifetime.Scoped)]
        public class ShoppingService : IShoppingService { }
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingService { }
    }
    [Fact]
    public void ImplementationScopeMoreRestrictedThanServiceScope()
    {
        CreateSetup<ImplementationScopeMoreRestrictedThanServiceScopeClasses>(out var ex);

        ex.GetException().Should().NotBeNull();
    }



    private class ImplementationHasOtherScopeThanServiceClasses
    {
        // provided as singleton
        [Service(ServiceLifetime.Singleton)]
        public class ShoppingService : IShoppingService { }
        // provided as scoped with ShoppingService as alias factory - this means the service will act like a singleton but be injected as scoped
        [Service(ServiceLifetime.Scoped)]
        public interface IShoppingService { }
    }

    [Fact]
    public void ImplementationHasOtherScopeThanService()
    {
        var provider = CreateSetup<ImplementationHasOtherScopeThanServiceClasses>(out var ex, out var services);

        var implementation = provider.GetRequiredService<ImplementationHasOtherScopeThanServiceClasses.ShoppingService>();
        implementation.Should().BeOfType<ImplementationHasOtherScopeThanServiceClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(ImplementationHasOtherScopeThanServiceClasses.ShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        var @interface = provider.CreateScope().ServiceProvider.GetRequiredService<ImplementationHasOtherScopeThanServiceClasses.IShoppingService>();
        @interface.Should().BeOfType<ImplementationHasOtherScopeThanServiceClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(ImplementationHasOtherScopeThanServiceClasses.IShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Scoped);


        @interface.Should().BeSameAs(implementation);

        ex.GetException().Should().BeNull();
    }



    private class MultipleServiceInterfacesOfDifferingScopeClasses
    {
        // provided as singleton
        public class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
        // provided as singleton with ShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingQueryService { }
        // provided as scoped with ShoppingService as alias factory
        [Service(ServiceLifetime.Scoped)]
        public interface IShoppingCommandService { }
    }

    [Fact]
    public void MultipleServiceInterfacesOfDifferingScope()
    {
        var provider = CreateSetup<MultipleServiceInterfacesOfDifferingScopeClasses>(out var ex, out var services);

        var implementation = provider.GetRequiredService<MultipleServiceInterfacesOfDifferingScopeClasses.ShoppingService>();
        implementation.Should().BeOfType<MultipleServiceInterfacesOfDifferingScopeClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(MultipleServiceInterfacesOfDifferingScopeClasses.ShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);



        var queryInterface = provider.CreateScope().ServiceProvider.GetRequiredService<MultipleServiceInterfacesOfDifferingScopeClasses.IShoppingQueryService>();
        queryInterface.Should().BeOfType<MultipleServiceInterfacesOfDifferingScopeClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(MultipleServiceInterfacesOfDifferingScopeClasses.IShoppingQueryService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);



        var commandInterface = provider.CreateScope().ServiceProvider.GetRequiredService<MultipleServiceInterfacesOfDifferingScopeClasses.IShoppingCommandService>();
        commandInterface.Should().BeOfType<MultipleServiceInterfacesOfDifferingScopeClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(MultipleServiceInterfacesOfDifferingScopeClasses.IShoppingCommandService))
            .Lifetime.Should().Be(ServiceLifetime.Scoped);


        commandInterface.Should().BeSameAs(queryInterface).And.BeSameAs(implementation);

        ex.GetException().Should().BeNull();
    }

    //undesirable:
    private class DuplicateServiceRegistrationClasses
    {
        // provided as singleton
        public class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
        // provided as singleton
        public class MockShoppingService : IShoppingQueryService, IShoppingCommandService { }
        // provided twice as singleton with ShoppingService as alias factory and MockShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingQueryService { }
        // provided twice as singleton with ShoppingService as alias factory and MockShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingCommandService { }
    }

    [Fact]
    public void DuplicateServiceRegistration()
    {
        var provider = CreateSetup<DuplicateServiceRegistrationClasses>(out var ex, out var services);
        var commandServices =
            services
                .Where(d => d.ServiceType == typeof(DuplicateServiceRegistrationClasses.IShoppingCommandService))
                .Select(p => new { Instance = p.ImplementationFactory?.Invoke(provider), p.ServiceType })
                .ToReadOnlyCollection();
        var queryServices =
            services
                .Where(d => d.ServiceType == typeof(DuplicateServiceRegistrationClasses.IShoppingQueryService))
                .Select(p => new { Instance = p.ImplementationFactory?.Invoke(provider), p.ServiceType })
                .ToReadOnlyCollection();
        // results in duplicate service registration
        commandServices.Count.Should().Be(2);
        commandServices.Count(b => b.Instance is DuplicateServiceRegistrationClasses.ShoppingService).Should().Be(1);
        commandServices.Count(b => b.Instance is DuplicateServiceRegistrationClasses.MockShoppingService).Should().Be(1);
        queryServices.Count.Should().Be(2);
        queryServices.Count(b => b.Instance is DuplicateServiceRegistrationClasses.ShoppingService).Should().Be(1);
        queryServices.Count(b => b.Instance is DuplicateServiceRegistrationClasses.MockShoppingService).Should().Be(1);
    }


    // solution:
    private class SimpleImplementationOverrideClasses
    {
        // provided as singleton
        public class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
        // provided as singleton
        [ServiceImplementationOverride] // add this attribute to define implementation priority
        public class MockShoppingService : IShoppingQueryService, IShoppingCommandService { }
        // provided as singleton with MockShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingQueryService { }
        // provided as singleton with MockShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingCommandService { }
    }

    [Fact]
    public void SimpleImplementationOverride()
    {
        var provider = CreateSetup<SimpleImplementationOverrideClasses>(out var ex, out var services);

        var implementation = provider.GetRequiredService<SimpleImplementationOverrideClasses.MockShoppingService>();
        implementation.Should().BeOfType<SimpleImplementationOverrideClasses.MockShoppingService>();

        new Action(
                () => provider.GetRequiredService<SimpleImplementationOverrideClasses.ShoppingService>()
                    .Should().BeOfType<SimpleImplementationOverrideClasses.ShoppingService>())
            .Should().Throw<InvalidOperationException>();


        services.Single(d => d.ServiceType == typeof(SimpleImplementationOverrideClasses.MockShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);



        var queryInterface = provider.CreateScope().ServiceProvider.GetRequiredService<SimpleImplementationOverrideClasses.IShoppingQueryService>();
        queryInterface.Should().BeOfType<SimpleImplementationOverrideClasses.MockShoppingService>();

        services.Single(d => d.ServiceType == typeof(SimpleImplementationOverrideClasses.IShoppingQueryService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);



        var commandInterface = provider.CreateScope().ServiceProvider.GetRequiredService<SimpleImplementationOverrideClasses.IShoppingCommandService>();
        commandInterface.Should().BeOfType<SimpleImplementationOverrideClasses.MockShoppingService>();

        services.Single(d => d.ServiceType == typeof(SimpleImplementationOverrideClasses.IShoppingCommandService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);


        commandInterface.Should().BeSameAs(queryInterface).And.BeSameAs(implementation);

        ex.GetException().Should().BeNull();
    }






    // undesirable:
    private class UndesirableDifferingServiceProviderClasses
    {
        // provided as singleton
        public class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
        // provided as singleton
        [ServiceImplementationOverride]
        public class MockShoppingService : IShoppingCommandService { }
        // provided as singleton with ShoppingService as alias factory - the non-mock implementation will be used since the mock does not implement this interface
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingQueryService { }
        // provided as singleton with MockShoppingService as alias factory
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingCommandService { }
    }
    [Fact]
    public void Undesirable_DifferingServiceProvider()
    {
        var provider = CreateSetup<UndesirableDifferingServiceProviderClasses>(out var ex, out var services);


        var implementation = provider.GetRequiredService<UndesirableDifferingServiceProviderClasses.ShoppingService>();
        implementation.Should().BeOfType<UndesirableDifferingServiceProviderClasses.ShoppingService>();

        var mockImplementation = provider.GetRequiredService<UndesirableDifferingServiceProviderClasses.MockShoppingService>();
        mockImplementation.Should().BeOfType<UndesirableDifferingServiceProviderClasses.MockShoppingService>();

        services.Single(d => d.ServiceType == typeof(UndesirableDifferingServiceProviderClasses.MockShoppingService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);



        var queryInterface = provider.CreateScope().ServiceProvider.GetRequiredService<UndesirableDifferingServiceProviderClasses.IShoppingQueryService>();
        queryInterface.Should().BeOfType<UndesirableDifferingServiceProviderClasses.ShoppingService>();

        services.Single(d => d.ServiceType == typeof(UndesirableDifferingServiceProviderClasses.IShoppingQueryService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);



        var commandInterface = provider.CreateScope().ServiceProvider.GetRequiredService<UndesirableDifferingServiceProviderClasses.IShoppingCommandService>();
        commandInterface.Should().BeOfType<UndesirableDifferingServiceProviderClasses.MockShoppingService>();

        services.Single(d => d.ServiceType == typeof(UndesirableDifferingServiceProviderClasses.IShoppingCommandService))
            .Lifetime.Should().Be(ServiceLifetime.Singleton);

        commandInterface.Should().BeSameAs(mockImplementation);
        queryInterface.Should().BeSameAs(implementation);

        ex.GetException().Should().BeNull();
    }

    // solution:
    private class ServiceImplementationValidationClasses
    {
        public class ShoppingService : IShoppingQueryService, IShoppingCommandService { }
        [ServiceImplementationOverride(typeof(ShoppingService))] // causes an exception to be thrown indicating the missing interface
        public class MockShoppingService : IShoppingQueryService { }
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingQueryService { }
        [Service(ServiceLifetime.Singleton)]
        public interface IShoppingCommandService { }
    }

    [Fact]
    public void ServiceImplementationValidation()
    {
        CreateSetup<ServiceImplementationValidationClasses>(out var ex, out _);
        ex.GetException().Should().NotBeNull();
    }


}
