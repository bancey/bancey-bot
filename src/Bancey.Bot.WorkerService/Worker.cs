namespace Bancey.Bot.WorkerService;

using Discord;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private IHostApplicationLifetime _hostApplicationLifetime;
    private TelemetryClient _telemetryClient;
    private IConfiguration _configuration;

    public DiscordWorker(ILogger<DiscordWorker> logger, IHostApplicationLifetime hostApplicationLifetime, TelemetryClient telemetryClient, IConfiguration configuration)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _telemetryClient = telemetryClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DiscordWorker starting at: {time}", DateTimeOffset.Now);

        BanceyBotSettings? settings = _configuration.GetRequiredSection("BanceyBot").Get<BanceyBotSettings>();

        if (settings == null)
        {
            _logger.LogError("Settings not found.");
            _hostApplicationLifetime.StopApplication();
            return;
        }

        if (string.IsNullOrEmpty(settings.DiscordToken))
        {
            _logger.LogError("Discord token not supplied or empty.");
            _hostApplicationLifetime.StopApplication();
            return;
        }

        BanceyBotLogger.InitLogger(_logger);
        var client = new DiscordSocketClient();
        client.Log += BanceyBotLogger.Log;
        await client.LoginAsync(TokenType.Bot, settings.DiscordToken);
        await client.StartAsync();

        await Task.Delay(-1, stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }
}
