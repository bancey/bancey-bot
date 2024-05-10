namespace Bancey.Bot.WorkerService;

public sealed class BanceyBotSettings
{
    public required string DiscordToken { get; set; }
    public required List<string> AllowedChannelIds { get; set; }
    public required List<string> AllowedRoleIds { get; set; }
    public required BanceyBotAzureSettings Azure { get; set; }
}