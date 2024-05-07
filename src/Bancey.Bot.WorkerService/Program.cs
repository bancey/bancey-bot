using Bancey.Bot.WorkerService;
using Discord.WebSocket;

var config = new DiscordSocketConfig()
{
  AlwaysDownloadUsers = true,
  MessageCacheSize = 100,
  GatewayIntents = Discord.GatewayIntents.GuildMembers
};

var builder = Host.CreateApplicationBuilder(args);
builder.Services
  .AddApplicationInsightsTelemetryWorkerService()
  .AddSingleton(config)
  .AddSingleton<DiscordSocketClient>()
  .AddSingleton<BanceyBot>();

builder.Services.AddHostedService<DiscordWorker>();

var host = builder.Build();
host.Run();
