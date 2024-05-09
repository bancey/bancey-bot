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
public class AzureCommands(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient, IConfiguration configuration, ArmClient armClient, IMemoryCache cache) : BanceyBotInteractionModuleBase(logger, telemetryClient, configuration)
{
  private readonly ArmClient _armClient = armClient;
  private IMemoryCache _cache = cache;

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

    using (_telemetryClient.StartOperation<RequestTelemetry>("GetServers"))
    {
      _logger.LogInformation("Getting servers matching configured tags. {tags}", formattedTags);
      await DeferAsync();
      var subscription = await GetSubscription();
      var virtualMachines = subscription.GetVirtualMachinesAsync();

      var embed = new EmbedBuilder()
        .WithTitle("Azure Servers")
        .WithColor(Color.DarkTeal);

      var stringBuilder = new StringBuilder();

      int count = 0;
      await foreach (var virtualMachine in virtualMachines)
      {
        if (virtualMachine.Data.Tags != null && _settings.Azure.Tags.All(tag => virtualMachine.Data.Tags.ContainsKey(tag.Key) && virtualMachine.Data.Tags[tag.Key] == tag.Value))
        {
          count++;
          stringBuilder
            .AppendLine($"### {virtualMachine.Data.Name}")
            .AppendLine($"**Status**: `{virtualMachine.Get().Value.InstanceView().Value.Statuses[1].DisplayStatus}`");
        }
      }
      stringBuilder.AppendLine();
      stringBuilder.AppendLine($"Start a server: </azure start-server:1237474161455267840>");

      embed.WithDescription(stringBuilder.ToString()).WithFooter($"Total: {count}, Tags: {string.Join(", ", formattedTags)}").WithCurrentTimestamp();
      await FollowupAsync(embeds: [embed.Build()]);
    }
  }

  [SlashCommand("start-server", "Starts a specific Azure VM.")]
  public async Task StartServer(string serverName)
  {
    await RespondAsync("Not yet implemented.");
  }

  private async Task<SubscriptionResource> GetSubscription()
  {
    if (_cache.TryGetValue("defaultSubscription", out object? value) &&
        value is SubscriptionResource cachedSubscription)
    {
      _logger.LogInformation("Subscription {name} returned from cache.", cachedSubscription.Data.DisplayName);
      return cachedSubscription;
    }

    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
      .SetSlidingExpiration(TimeSpan.FromMinutes(10))
      .RegisterPostEvictionCallback(OnPostEviction, _logger);
    SubscriptionResource subscription = await _armClient.GetDefaultSubscriptionAsync();
    _logger.LogInformation("Subscription {name} added to cache.", subscription.Data.DisplayName);
    _cache.Set("defaultSubscription", subscription, cacheEntryOptions);
    return subscription;
  }

  public static void OnPostEviction(object key, object? value, EvictionReason reason, object? state)
  {
    ILogger? logger = (ILogger?)state;
    if (value is SubscriptionResource sub)
    {
      logger?.LogInformation("Subscription {name} evicted from cache. Reason: {reason}", sub.Data.DisplayName, reason);
    }

    if (value is VirtualMachineResource vm)
    {
      logger?.LogInformation("Virtual Machine {name} evicted from cache. Reason: {reason}", vm.Data.Name, reason);
    }
  }
}