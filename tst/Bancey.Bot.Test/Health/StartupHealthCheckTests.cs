using Bancey.Bot.WorkerService.Health;
using Discord;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bancey.Bot.Test.Health;

public class StartupHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenClientConnected()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StartupHealthCheck>>();
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        var mockClient = new Mock<DiscordSocketClient>();
        
        mockClient.Setup(c => c.ConnectionState).Returns(ConnectionState.Connected);
        
        var healthCheck = new StartupHealthCheck(
            mockLogger.Object,
            telemetryClient,
            mockClient.Object);
        
        var context = new HealthCheckContext();
        
        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        
        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Theory]
    [InlineData(ConnectionState.Disconnected)]
    [InlineData(ConnectionState.Connecting)]
    [InlineData(ConnectionState.Disconnecting)]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenClientNotConnected(ConnectionState state)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StartupHealthCheck>>();
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        var mockClient = new Mock<DiscordSocketClient>();
        
        mockClient.Setup(c => c.ConnectionState).Returns(state);
        
        var healthCheck = new StartupHealthCheck(
            mockLogger.Object,
            telemetryClient,
            mockClient.Object);
        
        var context = new HealthCheckContext();
        
        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        
        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StartupHealthCheck>>();
        var telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        var mockClient = new Mock<DiscordSocketClient>();
        
        mockClient.Setup(c => c.ConnectionState).Returns(ConnectionState.Connected);
        
        var healthCheck = new StartupHealthCheck(
            mockLogger.Object,
            telemetryClient,
            mockClient.Object);
        
        var context = new HealthCheckContext();
        
        // Act
        await healthCheck.CheckHealthAsync(context);
        
        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Checking app health")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
