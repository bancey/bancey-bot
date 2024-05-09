using System.Text;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using Discord;
using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Caching.Memory;

namespace Bancey.Bot.WorkerService;

[Group("azure", "Azure Commands")]
public class AzureCommands(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient, IConfiguration configuration, ArmClient armClient, ResourceCacheManager<SubscriptionResource> subscriptionCache, ResourceCacheManager<VirtualMachineResource> vmCache) : BanceyBotInteractionModuleBase(logger, telemetryClient, configuration)
{
  private readonly ArmClient _armClient = armClient;
  private ResourceCacheManager<SubscriptionResource> _subscriptionCache = subscriptionCache;
  private ResourceCacheManager<VirtualMachineResource> _vmCache = vmCache;

  [SlashCommand("list-servers", "Lists servers matching configured tags.")]
  public async Task GetServers()
  {
    if (_settings == null || _settings.Azure == null || _settings.Azure.Tags == null || _settings.Azure.Tags.Count == 0)
    {
      _logger.LogError("Required Azure settings not found.");
      await RespondAsync("Please contact the bot owner, required settings are missing.");
      return;
    }

    var formattedTags = _settings.Azure.Tags.Select(tag => $"{tag.Key}={tag.Value}");

    var getServersOperation = _telemetryClient.StartOperation<RequestTelemetry>("GetServers");
    _logger.LogInformation("Getting servers matching configured tags. {tags}", formattedTags);
    await DeferAsync();
    var subscription = await GetSubscription();
    var virtualMachines = await GetVirtualMachines(subscription, _settings.Azure.Tags);

    var embed = new EmbedBuilder()
      .WithTitle("Azure Servers")
      .WithColor(Color.DarkTeal);

    var stringBuilder = new StringBuilder();
    foreach (var virtualMachine in virtualMachines)
    {
      stringBuilder
        .AppendLine($"### {virtualMachine.Data.Name}")
        .AppendLine($"**Status**: `{virtualMachine.Get().Value.InstanceView().Value.Statuses[1].DisplayStatus}`");
    }
    stringBuilder.AppendLine();
    stringBuilder.AppendLine($"Start a server: </azure start-server:1237474161455267840>");

    embed.WithDescription(stringBuilder.ToString()).WithFooter($"Total: {virtualMachines.Count}, Tags: {string.Join(", ", formattedTags)}").WithCurrentTimestamp();
    await FollowupAsync(embeds: [embed.Build()]);
    _telemetryClient.StopOperation(getServersOperation);

    var user = await Context.Guild.GetCurrentUserAsync();
  }

  [SlashCommand("start-server", "Starts a specific Azure VM.")]
  public async Task StartServer([Summary("serverName", "Attempts to start a server in Azure."), Autocomplete(typeof(VirtualMachineAutoCompleteHandler))] string serverName)
  {
    await RespondAsync($"Your choice: {serverName}");
  }

  private async Task<IList<VirtualMachineResource>> GetVirtualMachines(SubscriptionResource subscription, Dictionary<string, string> tags)
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

    _logger.LogInformation("No valid cache entries found, quering Azure...");
    var virtualMachines = subscription.GetVirtualMachinesAsync();
    await foreach (var virtualMachine in virtualMachines)
    {
      if (virtualMachine.Data.Tags != null && tags.All(tag => virtualMachine.Data.Tags.ContainsKey(tag.Key) && virtualMachine.Data.Tags[tag.Key] == tag.Value))
      {
        MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
          .SetSlidingExpiration(TimeSpan.FromMinutes(1));
        _vmCache.CacheResource(virtualMachine.Data.Name, virtualMachine, cacheEntryOptions);
        filteredVirtualMachines.Add(virtualMachine);
      }
    }
    return filteredVirtualMachines;
  }

  private async Task<SubscriptionResource> GetSubscription()
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