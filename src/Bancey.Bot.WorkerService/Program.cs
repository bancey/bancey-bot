using Azure.Identity;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using Bancey.Bot.WorkerService;
using Bancey.Bot.WorkerService.Azure;
using Bancey.Bot.WorkerService.Model;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Azure;

var discordConfig = new DiscordSocketConfig()
{
    AlwaysDownloadUsers = true,
    MessageCacheSize = 100,
    GatewayIntents = Discord.GatewayIntents.GuildMembers
};

var builder = Host.CreateApplicationBuilder(args);
builder.Services
  .AddApplicationInsightsTelemetryWorkerService()
  .AddMemoryCache()
  .AddSingleton(discordConfig)
  .AddSingleton<DiscordSocketClient>()
  .AddSingleton<InteractionService>()
  .AddSingleton<BanceyBot>()
  .AddSingleton<ResourceCacheManager<VirtualMachineResource>>()
  .AddSingleton<ResourceCacheManager<SubscriptionResource>>()
  .AddSingleton<AzureClient>();

var config = builder.Configuration.GetRequiredSection("BanceyBot").Get<BanceyBotSettings>();

if (config == null || config.Azure == null)
{
    throw new InvalidOperationException("Required Azure settings not found.");
}

builder.Services.AddAzureClients(builder =>
{
    builder.UseCredential(new ClientSecretCredential(config.Azure.TenantId, config.Azure.ClientId, config.Azure.ClientSecret));
    builder.AddArmClient(config.Azure.SubscriptionId);
});

if (config.Pterodactyl == null)
{
    throw new InvalidOperationException("Required Pterodactyl settings not found.");
}
var httpClient = new HttpClient
{
    BaseAddress = new Uri(config.Pterodactyl.BaseUrl)
};
httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.Pterodactyl.ClientToken}");

builder.Services.AddSingleton(httpClient);

builder.Services.AddHostedService<DiscordWorker>();

var host = builder.Build();
host.Run();
