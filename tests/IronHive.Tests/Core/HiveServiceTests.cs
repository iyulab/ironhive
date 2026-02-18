using FluentAssertions;
using IronHive.Abstractions;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Tools;
using IronHive.Core;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace IronHive.Tests.Core;

/// <summary>
/// Tests for HiveService core functionality.
/// P0-1.3: Core service initialization and dependency resolution tests.
/// </summary>
public class HiveServiceTests
{
    [Fact]
    public void Build_ShouldCreateHiveService_WithAllRequiredDependencies()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<HiveService>();
    }

    [Fact]
    public void Build_ShouldResolve_ProviderRegistry()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Providers.Should().NotBeNull();
        service.Providers.Should().BeAssignableTo<IProviderRegistry>();
    }

    [Fact]
    public void Build_ShouldResolve_StorageRegistry()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Storages.Should().NotBeNull();
        service.Storages.Should().BeAssignableTo<IStorageRegistry>();
    }

    [Fact]
    public void Build_ShouldResolve_ToolCollection()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Tools.Should().NotBeNull();
        service.Tools.Should().BeAssignableTo<IToolCollection>();
    }

    [Fact]
    public void Build_ShouldResolve_MemoryService()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Memory.Should().NotBeNull();
        service.Memory.Should().BeAssignableTo<IMemoryService>();
    }

    [Fact]
    public void Build_ShouldExpose_ServiceProvider()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act
        var service = builder.Build();

        // Assert
        service.Services.Should().NotBeNull();
        service.Services.Should().BeAssignableTo<IServiceProvider>();
    }

    [Fact]
    public void Services_Property_ShouldResolveDependencies()
    {
        // Arrange
        var builder = new HiveServiceBuilder();
        var service = builder.Build();

        // Act
        var providers = service.Services.GetService<IProviderRegistry>();
        var storages = service.Services.GetService<IStorageRegistry>();
        var tools = service.Services.GetService<IToolCollection>();

        // Assert
        providers.Should().NotBeNull();
        storages.Should().NotBeNull();
        tools.Should().NotBeNull();
    }

    [Fact]
    public void Build_MultipleTimes_ShouldCreateSeparateInstances()
    {
        // Arrange
        var builder1 = new HiveServiceBuilder();
        var builder2 = new HiveServiceBuilder();

        // Act
        var service1 = builder1.Build();
        var service2 = builder2.Build();

        // Assert
        service1.Should().NotBeSameAs(service2);
        service1.Providers.Should().NotBeSameAs(service2.Providers);
    }

    [Fact]
    public void HiveServiceBuilder_Services_ShouldBeAccessible()
    {
        // Arrange
        var builder = new HiveServiceBuilder();

        // Act & Assert
        builder.Services.Should().NotBeNull();
        builder.Services.Should().BeAssignableTo<IServiceCollection>();
    }

    [Fact]
    public void HiveServiceBuilder_ShouldAllowCustomServiceRegistration()
    {
        // Arrange
        var builder = new HiveServiceBuilder();
        var customService = new CustomTestService();
        builder.Services.AddSingleton<ICustomTestService>(customService);

        // Act
        var service = builder.Build();
        var resolved = service.Services.GetService<ICustomTestService>();

        // Assert
        resolved.Should().NotBeNull();
        resolved.Should().Be(customService);
    }
}

/// <summary>
/// Test interface for custom service registration tests.
/// </summary>
public interface ICustomTestService { }

/// <summary>
/// Test implementation for custom service registration tests.
/// </summary>
public class CustomTestService : ICustomTestService {
}
