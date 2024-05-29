namespace Bancey.Bot.WorkerService.Model;

public sealed class BanceyBotSettings
{
    public required string DiscordToken { get; set; }
    public ulong GuildId { get; set; } = ulong.MinValue;
    public required List<string> AllowedChannelIds { get; set; }
    public required List<string> AllowedRoleIds { get; set; }
    public required BanceyBotAzureSettings Azure { get; set; }
    public required BanceyBotPterodactylSettings Pterodactyl { get; set; }
}