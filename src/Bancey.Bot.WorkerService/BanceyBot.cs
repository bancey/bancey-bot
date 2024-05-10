using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

public class BanceyBot
{
    private IServiceProvider _serviceProvider;
    private readonly ILogger<BanceyBot> _logger;
    private TelemetryClient _telemetryClient;
    private DiscordSocketClient _client;
    private InteractionService _interactionService;

    public BanceyBot(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<BanceyBot>>();
        _telemetryClient = _serviceProvider.GetRequiredService<TelemetryClient>();
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
        _interactionService = _serviceProvider.GetRequiredService<InteractionService>();

        if (!BanceyBotLogger.IsInitialized())
        {
            BanceyBotLogger.InitLogger(_logger);
        }

        _client.Log += BanceyBotLogger.Log;
        _client.Ready += OnClientReady;
    }

    public async Task LoginAndStart(string token)
    {
        var loginOperation = _telemetryClient.StartOperation<RequestTelemetry>("LoginAndStart");
        _logger.LogInformation("Logging in to Discord...");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        _logger.LogInformation("Logged in to Discord.");
        _telemetryClient.StopOperation(loginOperation);
    }

    public async Task Stop()
    {
        var stopOperation = _telemetryClient.StartOperation<RequestTelemetry>("Stop");
        _logger.LogInformation("Stopping Discord client...");
        await _client.StopAsync();
        _logger.LogInformation("Stopped Discord client.");
        _telemetryClient.StopOperation(stopOperation);
    }

    public async Task OnClientReady()
    {
        var registerCommandsOperation = _telemetryClient.StartOperation<RequestTelemetry>("RegisterCommands");
        _logger.LogInformation("Registering commands...");
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        var result = await _interactionService.RegisterCommandsToGuildAsync(423809074400854036);
        _client.InteractionCreated += InteractionCreated;
        _logger.LogInformation("Commands successfully registered: {count}", result.Count);
        _telemetryClient.StopOperation(registerCommandsOperation);
    }

    public async Task InteractionCreated(SocketInteraction interaction)
    {
        if (_interactionService == null)
        {
            _logger.LogError("Interaction service not initialized.");
            return;
        }

        var interactionCreatedOperation = _telemetryClient.StartOperation<RequestTelemetry>("InteractionCreated");
        _logger.LogInformation("Interaction executed {id}.", interaction.Id);
        var scope = _serviceProvider.CreateScope();
        var ctx = new SocketInteractionContext(_client, interaction);
        await _interactionService.ExecuteCommandAsync(ctx, scope.ServiceProvider);
        _telemetryClient.StopOperation(interactionCreatedOperation);
    }
}