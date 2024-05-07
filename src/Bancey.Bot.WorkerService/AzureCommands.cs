using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

[Group("azure", "Azure Commands")]
public class AzureCommands(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient, IConfiguration configuration, ArmClient armClient) : BanceyBotInteractionModuleBase(logger, telemetryClient, configuration)
{
  private readonly ArmClient _armClient = armClient;

    [SlashCommand("list-servers", "Lists servers matching configured tags.")]
  public async Task GetServers()
  {
    if (_settings == null || _settings.Azure == null || _settings.Azure.Tags == null || _settings.Azure.Tags.Count == 0)
    {
      _logger.LogError("Required Azure settings not found.");
      await RespondAsync("Please contact the bot owner, required settings are missing.");
      return;
    }

    using (_telemetryClient.StartOperation<RequestTelemetry>("GetServers"))
    {
      _logger.LogInformation($"Getting servers matching configured tags. {_settings.Azure.Tags}");
      await DeferAsync();
      var subscription = await _armClient.GetDefaultSubscriptionAsync();
      var resourceGroups = subscription.GetResourceGroups();
      var count = 0;
      await foreach(ResourceGroupResource resourceGroup in resourceGroups)
      {
        var virtualMachines = resourceGroup.GetVirtualMachines();
        count += await virtualMachines.CountAsync();
      }
      await FollowupAsync($"Found {count} servers from {await resourceGroups.CountAsync()} resource groups. matching configured tags.");
    }
  }
}