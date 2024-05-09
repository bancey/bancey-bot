using Azure.Identity;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Resources;
using Bancey.Bot.WorkerService;
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
  .AddSingleton<ResourceCacheManager<SubscriptionResource>>();

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

builder.Services.AddHostedService<DiscordWorker>();

var host = builder.Build();
host.Run();
