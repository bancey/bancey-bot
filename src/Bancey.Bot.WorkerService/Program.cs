using Azure.Identity;
using Bancey.Bot.WorkerService;
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
  .AddSingleton(discordConfig)
  .AddSingleton<DiscordSocketClient>()
  .AddSingleton<BanceyBot>();

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
