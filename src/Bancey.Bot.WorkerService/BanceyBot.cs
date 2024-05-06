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

  public BanceyBot(ILogger<BanceyBot> logger, TelemetryClient telemetryClient)
  {
    _logger = logger;
    _telemetryClient = telemetryClient;

    if (!BanceyBotLogger.IsInitialized())
    {
      BanceyBotLogger.InitLogger(_logger);
    }

    var config = new DiscordSocketConfig
    {
      AlwaysDownloadUsers = true,
      MessageCacheSize = 100,
    };
    _client = new DiscordSocketClient(config);
    _client.Log += BanceyBotLogger.Log;
    _client.Ready += OnClientReady;
  }

  public async Task LoginAndStart(string token)
  {
    await _client.LoginAsync(TokenType.Bot, token);
    await _client.StartAsync();
  }

  public async Task OnClientReady()
  {
    using(var registerCommandsOperation = _telemetryClient.StartOperation<RequestTelemetry>("RegisterCommands"))
    {
      _logger.LogInformation("Registering commands...");
      var interactionService = new InteractionService(_client);
      var result = await interactionService.RegisterCommandsGloballyAsync();
      _logger.LogInformation("Commands successfully registered: {count}", result.Count);
    }
  }
}