using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

public class BanceyBot
{
  private readonly ILogger<BanceyBot> _logger;
  private TelemetryClient _telemetryClient;
  private DiscordSocketClient _client;

  public BanceyBot(ILogger<BanceyBot> logger, TelemetryClient telemetryClient, DiscordSocketClient client)
  {
    _logger = logger;
    _telemetryClient = telemetryClient;
    _client = client;

    if (!BanceyBotLogger.IsInitialized())
    {
      BanceyBotLogger.InitLogger(_logger);
    }

    _client.Log += BanceyBotLogger.Log;
    _client.Ready += OnClientReady;
  }

  public async Task LoginAndStart(string token)
  {
    using(var loginOperation = _telemetryClient.StartOperation<RequestTelemetry>("LoginAndStart"))
    {
      _logger.LogInformation("Logging in to Discord...");
      await _client.LoginAsync(TokenType.Bot, token);
      await _client.StartAsync();
      _logger.LogInformation("Logged in to Discord.");
    }
  }

  public async Task OnClientReady()
  {
    using(var registerCommandsOperation = _telemetryClient.StartOperation<RequestTelemetry>("RegisterCommands"))
    {
      _logger.LogInformation("Registering commands...");
      var interactionService = new InteractionService(_client.Rest);
      var result = await interactionService.RegisterCommandsToGuildAsync(423809074400854036);
      _logger.LogInformation("Commands successfully registered: {count}", result.Count);
    }
  }
}