using Bancey.Bot.WorkerService.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bancey.Bot.Test.Health;

public class FileHealthCheckPublisherTests
{
    [Fact]
    public async Task PublishAsync_DoesNotThrow_WhenHealthy()
    {
        // Arrange
        var logger = NullLogger<FileHealthCheckPublisher>.Instance;
        var publisher = new FileHealthCheckPublisher(logger);
        
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Healthy,
            TimeSpan.Zero);
        
        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => publisher.PublishAsync(healthReport, CancellationToken.None));
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_DoesNotThrow_WhenUnhealthy()
    {
        // Arrange
        var logger = NullLogger<FileHealthCheckPublisher>.Instance;
        var publisher = new FileHealthCheckPublisher(logger);
        
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Unhealthy,
            TimeSpan.Zero);
        
        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => publisher.PublishAsync(healthReport, CancellationToken.None));
        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_DoesNotThrow_WhenDegraded()
    {
        // Arrange
        var logger = NullLogger<FileHealthCheckPublisher>.Instance;
        var publisher = new FileHealthCheckPublisher(logger);
        
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Degraded,
            TimeSpan.Zero);
        
        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => publisher.PublishAsync(healthReport, CancellationToken.None));
        Assert.Null(exception);
    }
}
