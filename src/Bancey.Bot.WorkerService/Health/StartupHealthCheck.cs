using Discord;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bancey.Bot.WorkerService.Health;

internal sealed class StartupHealthCheck(ILogger<StartupHealthCheck> logger, TelemetryClient telemetryClient, DiscordSocketClient client) : IHealthCheck
{
  private readonly ILogger<StartupHealthCheck> _logger = logger;
  private readonly TelemetryClient _telemetryClient = telemetryClient;
  private readonly DiscordSocketClient _client = client;

  public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    var healthCheckOperation = _telemetryClient.StartOperation<RequestTelemetry>("HealthCheck");
    _logger.LogInformation("Checking app health...");
    var result = _client.ConnectionState == ConnectionState.Connected ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    _telemetryClient.StopOperation(healthCheckOperation);
    return Task.FromResult(result);
  }
}