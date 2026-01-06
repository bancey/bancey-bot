using Azure.ResourceManager;
using Bancey.Bot.WorkerService.Azure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bancey.Bot.Test.Azure;

public class ResourceCacheManagerTests
{
    [Fact]
    public void CachedKeys_ReturnsEmptyList_Initially()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResourceCacheManager<ArmResource>>>();
        var mockCache = new Mock<IMemoryCache>();
        var manager = new ResourceCacheManager<ArmResource>(mockLogger.Object, mockCache.Object);
        
        // Act
        var keys = manager.CachedKeys();
        
        // Assert
        Assert.NotNull(keys);
        Assert.Empty(keys);
    }

    [Fact]
    public void CacheResource_AddsKeyToCachedKeys()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResourceCacheManager<ArmResource>>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var manager = new ResourceCacheManager<ArmResource>(mockLogger.Object, cache);
        
        var mockResource = new Mock<ArmResource>();
        var options = new MemoryCacheEntryOptions();
        
        // Act
        manager.CacheResource("testKey", mockResource.Object, options);
        
        // Assert
        var keys = manager.CachedKeys();
        Assert.Contains("testKey", keys);
    }

    [Fact]
    public void TryGetValue_ReturnsTrue_WhenKeyExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResourceCacheManager<ArmResource>>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var manager = new ResourceCacheManager<ArmResource>(mockLogger.Object, cache);
        
        var mockResource = new Mock<ArmResource>();
        var options = new MemoryCacheEntryOptions();
        manager.CacheResource("testKey", mockResource.Object, options);
        
        // Act
        var result = manager.TryGetValue("testKey", out var value);
        
        // Assert
        Assert.True(result);
        Assert.NotNull(value);
    }

    [Fact]
    public void TryGetValue_ReturnsFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResourceCacheManager<ArmResource>>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var manager = new ResourceCacheManager<ArmResource>(mockLogger.Object, cache);
        
        // Act
        var result = manager.TryGetValue("nonexistentKey", out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void OnPostEviction_RemovesKeyFromCachedKeys()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResourceCacheManager<ArmResource>>>();
        var mockResource = new Mock<ArmResource>();
        
        // Manually add a key
        var manager = new ResourceCacheManager<ArmResource>(mockLogger.Object, new Mock<IMemoryCache>().Object);
        var keys = manager.CachedKeys();
        keys.Add("testKey");
        
        // Act
        ResourceCacheManager<ArmResource>.OnPostEviction("testKey", mockResource.Object, EvictionReason.Expired, mockLogger.Object);
        
        // Assert
        Assert.DoesNotContain("testKey", keys);
    }

    [Fact]
    public void OnPostEviction_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ResourceCacheManager<ArmResource>>>();
        var mockResource = new Mock<ArmResource>();
        
        // Act
        ResourceCacheManager<ArmResource>.OnPostEviction("testKey", mockResource.Object, EvictionReason.Expired, mockLogger.Object);
        
        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("evicted from cache")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
