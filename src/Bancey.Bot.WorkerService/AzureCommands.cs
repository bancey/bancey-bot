using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using Discord;
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

    var formattedTags = _settings.Azure.Tags.Select(tag => $"{tag.Key}={tag.Value}");

    using (_telemetryClient.StartOperation<RequestTelemetry>("GetServers"))
    {
      _logger.LogInformation("Getting servers matching configured tags. {tags}", formattedTags);
      await DeferAsync();
      var subscription = await _armClient.GetDefaultSubscriptionAsync();
      var virtualMachines = subscription.GetVirtualMachinesAsync();
      var count = 0;
      await foreach (var virtualMachine in virtualMachines)
      {
        if (virtualMachine.Data.Tags != null && _settings.Azure.Tags.All(tag => virtualMachine.Data.Tags.ContainsKey(tag.Key) && virtualMachine.Data.Tags[tag.Key] == tag.Value))
        {
          count++;
        }
      }
      var embed = new EmbedBuilder()
        .WithTitle("Azure Servers")
        .AddField("Count", count)
        .Build();
      await FollowupAsync(embeds: [embed]);
    }
  }
}