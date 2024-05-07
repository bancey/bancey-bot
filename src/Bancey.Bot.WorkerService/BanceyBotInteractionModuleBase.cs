using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

public abstract class BanceyBotInteractionModuleBase : InteractionModuleBase
{
  internal readonly ILogger<BanceyBotInteractionModuleBase> _logger;
  internal readonly TelemetryClient _telemetryClient;

  public BanceyBotInteractionModuleBase(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient)
  {
    _logger = logger;
    _telemetryClient = telemetryClient;
  }

  public override void BeforeExecute(ICommandInfo command)
  {
    _logger.LogInformation("Executing command: {command}", command.Name);
    base.BeforeExecute(command);
  }

  public override void AfterExecute(ICommandInfo command)
  {
    _logger.LogInformation("Finished executing command: {command}", command.Name);
    base.AfterExecute(command);
  }
}