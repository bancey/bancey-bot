using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Caching.Memory;

namespace Bancey.Bot.WorkerService.Azure;

public class AzureClient(ILogger<AzureClient> logger, ArmClient armClient, ResourceCacheManager<SubscriptionResource> subscriptionCache, ResourceCacheManager<VirtualMachineResource> vmCache)
{
    private readonly ILogger<AzureClient> _logger = logger;
    private readonly ArmClient _armClient = armClient;
    private readonly ResourceCacheManager<SubscriptionResource> _subscriptionCache = subscriptionCache;
    private readonly ResourceCacheManager<VirtualMachineResource> _vmCache = vmCache;

    public async Task<VirtualMachineResource?> GetVirtualMachineAsync(SubscriptionResource subscription, string serverName)
    {
        if (_vmCache.TryGetValue(serverName, out VirtualMachineResource? value) && value != null)
        {
            _logger.LogInformation("Virtual Machine {name} returned from cache.", value.Data.Name);
            return value;
        }

        var virtualMachine = await subscription.GetVirtualMachinesAsync(filter: $"name eq '{serverName}'").FirstOrDefaultAsync();

        if (virtualMachine == null) return null;

        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(3));
        _vmCache.CacheResource(serverName, virtualMachine, cacheEntryOptions);
        return virtualMachine;
    }

    public async Task<IList<VirtualMachineResource>> GetVirtualMachines(SubscriptionResource subscription, Dictionary<string, string> tags)
    {
        var filteredVirtualMachines = new List<VirtualMachineResource>();
        foreach (string key in _vmCache.CachedKeys())
        {
            if (_vmCache.TryGetValue(key, out VirtualMachineResource? value) && value != null)
            {
                _logger.LogInformation("Virtual Machine {name} returned from cache.", value.Data.Name);
                filteredVirtualMachines.Add(value);
            }
        }

        if (filteredVirtualMachines.Count > 0)
        {
            _logger.LogInformation("Returning {count} virtual machines from cache.", filteredVirtualMachines.Count);
            return filteredVirtualMachines;
        }

        _logger.LogInformation("No valid cache entries found, querying Azure...");
        var virtualMachines = subscription.GetVirtualMachinesAsync();
        await foreach (var virtualMachine in virtualMachines)
        {
            if (virtualMachine.Data.Tags != null && tags.All(tag => virtualMachine.Data.Tags.ContainsKey(tag.Key) && virtualMachine.Data.Tags[tag.Key] == tag.Value))
            {
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                  .SetSlidingExpiration(TimeSpan.FromMinutes(3));
                _vmCache.CacheResource(virtualMachine.Data.Name, virtualMachine, cacheEntryOptions);
                filteredVirtualMachines.Add(virtualMachine);
            }
        }
        return filteredVirtualMachines;
    }

    public async Task<SubscriptionResource> GetSubscription()
    {
        if (_subscriptionCache.TryGetValue("defaultSubscription", out SubscriptionResource? value) && value != null)
        {
            _logger.LogInformation("Subscription {name} returned from cache.", value.Data.DisplayName);
            return value;
        }

        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(10));
        SubscriptionResource subscription = await _armClient.GetDefaultSubscriptionAsync();
        _logger.LogInformation("Subscription {name} added to cache.", subscription.Data.DisplayName);
        _subscriptionCache.CacheResource("defaultSubscription", subscription, cacheEntryOptions);
        return subscription;
    }
}