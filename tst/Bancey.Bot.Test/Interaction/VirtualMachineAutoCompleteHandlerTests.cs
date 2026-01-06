using Azure.ResourceManager.Compute;
using Bancey.Bot.WorkerService.Azure;
using Bancey.Bot.WorkerService.Interaction;
using Discord;
using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Bancey.Bot.Test.Interaction;

public class VirtualMachineAutoCompleteHandlerTests
{
    [Fact]
    public async Task GenerateSuggestionsAsync_ReturnsSuccess_WhenNoCachedVMs()
    {
        // Arrange
        var logger = NullLogger<VirtualMachineAutoCompleteHandler>.Instance;
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        var cacheLogger = NullLogger<ResourceCacheManager<VirtualMachineResource>>.Instance;
        var cache = new MemoryCache(new MemoryCacheOptions());
        var vmCache = new ResourceCacheManager<VirtualMachineResource>(cacheLogger, cache);
        
        var handler = new VirtualMachineAutoCompleteHandler(logger, telemetryClient, vmCache);
        
        var mockContext = new Mock<IInteractionContext>();
        var mockAutocomplete = new Mock<IAutocompleteInteraction>();
        var mockParameter = new Mock<IParameterInfo>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        // Act
        var result = await handler.GenerateSuggestionsAsync(
            mockContext.Object,
            mockAutocomplete.Object,
            mockParameter.Object,
            mockServiceProvider.Object);
        
        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GenerateSuggestionsAsync_ReturnsSuccess_WithCachedVMs()
    {
        // Arrange
        var logger = NullLogger<VirtualMachineAutoCompleteHandler>.Instance;
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        var cacheLogger = NullLogger<ResourceCacheManager<VirtualMachineResource>>.Instance;
        var cache = new MemoryCache(new MemoryCacheOptions());
        var vmCache = new ResourceCacheManager<VirtualMachineResource>(cacheLogger, cache);
        
        // Add some mock VM names to the cache
        var mockVM = new Mock<VirtualMachineResource>();
        vmCache.CacheResource("vm1", mockVM.Object, new MemoryCacheEntryOptions());
        vmCache.CacheResource("vm2", mockVM.Object, new MemoryCacheEntryOptions());
        vmCache.CacheResource("vm3", mockVM.Object, new MemoryCacheEntryOptions());
        
        var handler = new VirtualMachineAutoCompleteHandler(logger, telemetryClient, vmCache);
        
        var mockContext = new Mock<IInteractionContext>();
        var mockAutocomplete = new Mock<IAutocompleteInteraction>();
        var mockParameter = new Mock<IParameterInfo>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        // Act
        var result = await handler.GenerateSuggestionsAsync(
            mockContext.Object,
            mockAutocomplete.Object,
            mockParameter.Object,
            mockServiceProvider.Object);
        
        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GenerateSuggestionsAsync_ReturnsSuccess_WithManyCachedVMs()
    {
        // Arrange
        var logger = NullLogger<VirtualMachineAutoCompleteHandler>.Instance;
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        var cacheLogger = NullLogger<ResourceCacheManager<VirtualMachineResource>>.Instance;
        var cache = new MemoryCache(new MemoryCacheOptions());
        var vmCache = new ResourceCacheManager<VirtualMachineResource>(cacheLogger, cache);
        
        // Add 30 VMs to test the limit
        var mockVM = new Mock<VirtualMachineResource>();
        for (int i = 0; i < 30; i++)
        {
            vmCache.CacheResource($"vm{i}", mockVM.Object, new MemoryCacheEntryOptions());
        }
        
        var handler = new VirtualMachineAutoCompleteHandler(logger, telemetryClient, vmCache);
        
        var mockContext = new Mock<IInteractionContext>();
        var mockAutocomplete = new Mock<IAutocompleteInteraction>();
        var mockParameter = new Mock<IParameterInfo>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        // Act
        var result = await handler.GenerateSuggestionsAsync(
            mockContext.Object,
            mockAutocomplete.Object,
            mockParameter.Object,
            mockServiceProvider.Object);
        
        // Assert
        Assert.True(result.IsSuccess);
    }
}
