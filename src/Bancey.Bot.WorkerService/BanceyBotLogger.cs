namespace Bancey.Bot.WorkerService;

using Discord;

public static class BanceyBotLogger
{
  private static ILogger? logger;

  public static void InitLogger(ILogger logger)
  {
    BanceyBotLogger.logger = logger;
  }

  public static bool IsInitialized()
  {
    return logger != null;
  }

  public static Task Log(LogMessage message)
  {
    if (logger == null)
    {
      Console.WriteLine("Logger not initialized.");
      return Task.CompletedTask;
    }

    var severity = message.Severity switch
    {
      LogSeverity.Critical => LogLevel.Critical,
      LogSeverity.Error => LogLevel.Error,
      LogSeverity.Warning => LogLevel.Warning,
      LogSeverity.Info => LogLevel.Information,
      LogSeverity.Verbose => LogLevel.Trace,
      LogSeverity.Debug => LogLevel.Debug,
      _ => LogLevel.None
    };

    logger.Log(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
    return Task.CompletedTask;
  }
}