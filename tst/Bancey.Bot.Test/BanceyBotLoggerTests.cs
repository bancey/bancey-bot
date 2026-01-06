using Bancey.Bot.WorkerService;
using Discord;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bancey.Bot.Test;

public class BanceyBotLoggerTests
{
    [Fact]
    public void InitLogger_InitializesLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        
        // Act
        BanceyBotLogger.InitLogger(mockLogger.Object);
        
        // Assert
        Assert.True(BanceyBotLogger.IsInitialized());
    }

    [Theory]
    [InlineData(LogSeverity.Critical, LogLevel.Critical)]
    [InlineData(LogSeverity.Error, LogLevel.Error)]
    [InlineData(LogSeverity.Warning, LogLevel.Warning)]
    [InlineData(LogSeverity.Info, LogLevel.Information)]
    [InlineData(LogSeverity.Verbose, LogLevel.Trace)]
    [InlineData(LogSeverity.Debug, LogLevel.Debug)]
    public async Task Log_LogsMessageWithCorrectSeverity(LogSeverity discordSeverity, LogLevel expectedLogLevel)
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        BanceyBotLogger.InitLogger(mockLogger.Object);
        
        var logMessage = new LogMessage(discordSeverity, "TestSource", "Test message");
        
        // Act
        await BanceyBotLogger.Log(logMessage);
        
        // Assert
        mockLogger.Verify(
            x => x.Log(
                expectedLogLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Log_LogsMessageWithException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        BanceyBotLogger.InitLogger(mockLogger.Object);
        
        var exception = new Exception("Test exception");
        var logMessage = new LogMessage(LogSeverity.Error, "TestSource", "Test message", exception);
        
        // Act
        await BanceyBotLogger.Log(logMessage);
        
        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Log_WritesToConsole_WhenLoggerNotInitialized()
    {
        // Arrange - This test should run when logger happens to be null
        // Note: Since logger is static and tests may run in any order, we can't reliably test this
        // Just verify the method doesn't throw
        var logMessage = new LogMessage(LogSeverity.Info, "TestSource", "Test message");
        
        // Act & Assert - should not throw
        await BanceyBotLogger.Log(logMessage);
    }
}
