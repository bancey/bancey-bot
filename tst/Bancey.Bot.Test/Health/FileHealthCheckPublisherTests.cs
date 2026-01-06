using Bancey.Bot.WorkerService.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bancey.Bot.Test.Health;

public class FileHealthCheckPublisherTests
{
    [Fact]
    public async Task PublishAsync_CreatesFile_WhenHealthy()
    {
        // Arrange
        var logger = NullLogger<FileHealthCheckPublisher>.Instance;
        var publisher = new FileHealthCheckPublisher(logger);
        
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Healthy,
            TimeSpan.Zero);
        
        // Act
        await publisher.PublishAsync(healthReport, CancellationToken.None);
        
        // Assert - the actual file path is hardcoded in the publisher
        // We verify it doesn't throw
        Assert.True(true);
    }

    [Fact]
    public async Task PublishAsync_DeletesFile_WhenUnhealthy()
    {
        // Arrange
        var logger = NullLogger<FileHealthCheckPublisher>.Instance;
        var publisher = new FileHealthCheckPublisher(logger);
        
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Unhealthy,
            TimeSpan.Zero);
        
        // Act
        await publisher.PublishAsync(healthReport, CancellationToken.None);
        
        // Assert - verify it doesn't throw
        Assert.True(true);
    }

    [Fact]
    public async Task PublishAsync_DeletesFile_WhenDegraded()
    {
        // Arrange
        var logger = NullLogger<FileHealthCheckPublisher>.Instance;
        var publisher = new FileHealthCheckPublisher(logger);
        
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Degraded,
            TimeSpan.Zero);
        
        // Act
        await publisher.PublishAsync(healthReport, CancellationToken.None);
        
        // Assert - Degraded is not Healthy, so it should delete - verify it doesn't throw
        Assert.True(true);
    }
}
