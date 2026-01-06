using Bancey.Bot.WorkerService;
using Bancey.Bot.WorkerService.Model;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bancey.Bot.Test;

public class BanceyBotTests
{
    private IServiceProvider CreateServiceProvider(Dictionary<string, string?>? configValues = null)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues ?? new Dictionary<string, string?>())
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(new TelemetryClient(new TelemetryConfiguration()));
        services.AddSingleton(new DiscordSocketClient());
        services.AddSingleton(new InteractionService(new DiscordSocketClient()));
        
        return services.BuildServiceProvider();
    }

    [Fact]
    public void BanceyBot_Constructor_ThrowsWhenSettingsNull()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new BanceyBot(serviceProvider));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void BanceyBot_Constructor_SucceedsWithValidSettings()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["BanceyBot:DiscordToken"] = "test-token",
            ["BanceyBot:GuildId"] = "123456",
            ["BanceyBot:AllowedChannelIds:0"] = "channel1",
            ["BanceyBot:AllowedRoleIds:0"] = "role1",
            ["BanceyBot:Azure:TenantId"] = "tenant-id",
            ["BanceyBot:Azure:ClientId"] = "client-id",
            ["BanceyBot:Azure:ClientSecret"] = "client-secret",
            ["BanceyBot:Azure:SubscriptionId"] = "sub-id",
            ["BanceyBot:Pterodactyl:BaseUrl"] = "https://example.com",
            ["BanceyBot:Pterodactyl:ClientToken"] = "token"
        };
        
        var serviceProvider = CreateServiceProvider(config);
        
        // Act
        var bot = new BanceyBot(serviceProvider);
        
        // Assert
        Assert.NotNull(bot);
    }

    [Fact]
    public void BanceyBot_Constructor_InitializesLogger()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["BanceyBot:DiscordToken"] = "test-token",
            ["BanceyBot:AllowedChannelIds:0"] = "channel1",
            ["BanceyBot:AllowedRoleIds:0"] = "role1",
            ["BanceyBot:Azure:TenantId"] = "tenant-id",
            ["BanceyBot:Azure:ClientId"] = "client-id",
            ["BanceyBot:Azure:ClientSecret"] = "client-secret",
            ["BanceyBot:Azure:SubscriptionId"] = "sub-id",
            ["BanceyBot:Pterodactyl:BaseUrl"] = "https://example.com",
            ["BanceyBot:Pterodactyl:ClientToken"] = "token"
        };
        
        var serviceProvider = CreateServiceProvider(config);
        
        // Act
        var bot = new BanceyBot(serviceProvider);
        
        // Assert - BanceyBotLogger should be initialized
        Assert.True(BanceyBotLogger.IsInitialized());
    }
}
