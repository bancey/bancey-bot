using Bancey.Bot.WorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.AddHostedService<DiscordWorker>();

var host = builder.Build();
host.Run();
