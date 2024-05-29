using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bancey.Bot.WorkerService.Health;

internal sealed class FileHealthCheckPublisher(ILogger<FileHealthCheckPublisher> logger) : IHealthCheckPublisher
{
  private readonly ILogger<FileHealthCheckPublisher> _logger = logger;
  private readonly string _healthCheckFilePath = "/tmp/bancey-bot-health";

  public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
  {
    if (report.Status == HealthStatus.Healthy)
    {
      _logger.LogInformation("Health check is healthy. Touching health check file.");
      Touch(_healthCheckFilePath);
    }
    else
    {
      _logger.LogError("Health check is unhealthy. Removing health check file.");
      Delete(_healthCheckFilePath);
    }

    return Task.CompletedTask;
  }

  private static void Touch(string path)
  {
    using var fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
  }

  private static void Delete(string path)
  {
    try
    {
      File.Delete(path);
    }
    catch
    {
      // best effort delete; might not exist in the first place
    }
  }
}