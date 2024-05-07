using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

[Group("azure", "Azure Commands")]
public class AzureCommands(ILogger<AzureCommands> logger, TelemetryClient telemetryClient, IConfiguration configuration) : BanceyBotInteractionModuleBase(logger, telemetryClient, configuration)
{
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
      _logger.LogInformation("Getting servers matching configured tags. {tags}", _settings.Azure.Tags.ToString());
      await DeferAsync();
      await Task.Delay(5000);
      await FollowupAsync("No servers found.");
    }
  }
}