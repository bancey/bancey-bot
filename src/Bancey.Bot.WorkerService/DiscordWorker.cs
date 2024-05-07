namespace Bancey.Bot.WorkerService;

using Discord;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private IHostApplicationLifetime _hostApplicationLifetime;
    private IConfiguration _configuration;
    private BanceyBot _banceyBot;

    public DiscordWorker(ILogger<DiscordWorker> logger, IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, BanceyBot banceyBot)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
        _banceyBot = banceyBot;
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

        await _banceyBot.LoginAndStart(settings.DiscordToken);

        await Task.Delay(-1, stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }
}
