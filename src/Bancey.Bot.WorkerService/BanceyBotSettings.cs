namespace Bancey.Bot.WorkerService;

public sealed class BanceyBotSettings
{
    public required string DiscordToken { get; set; }
    public required BanceyBotAzureSettings Azure { get; set; }
}