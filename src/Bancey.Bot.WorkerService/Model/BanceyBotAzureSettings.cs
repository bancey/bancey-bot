namespace Bancey.Bot.WorkerService.Model;

public sealed class BanceyBotAzureSettings
{
    public required string TenantId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string SubscriptionId { get; set; }
    public required Dictionary<string, string> Tags { get; set; }
}