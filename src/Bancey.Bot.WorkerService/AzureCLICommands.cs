using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

public class AzureCLICommands(ILogger<AzureCLICommands> logger, TelemetryClient telemetryClient) : BanceyBotInteractionModuleBase(logger, telemetryClient)
{
  [SlashCommand("echo", "Echoes back the input.")]
  public async Task Echo(string input)
  {
    using (_telemetryClient.StartOperation<RequestTelemetry>("Echo"))
    {
      _logger.LogInformation("Echoing input: {input}", input);
      await RespondAsync(input);
    }
  }
}