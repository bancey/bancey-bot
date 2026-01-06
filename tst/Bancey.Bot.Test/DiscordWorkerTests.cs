using Bancey.Bot.WorkerService;
using Bancey.Bot.WorkerService.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Bancey.Bot.Test;

public class DiscordWorkerTests
{
    [Fact]
    public void DiscordWorker_CanBeConstructed()
    {
        // Arrange
        var logger = NullLogger<DiscordWorker>.Instance;
        var mockLifetime = new Mock<IHostApplicationLifetime>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BanceyBot:DiscordToken"] = "test-token",
                ["BanceyBot:AllowedChannelIds:0"] = "123",
                ["BanceyBot:AllowedRoleIds:0"] = "role1",
                ["BanceyBot:Azure:TenantId"] = "tenant",
                ["BanceyBot:Azure:ClientId"] = "client",
                ["BanceyBot:Azure:ClientSecret"] = "secret",
                ["BanceyBot:Azure:SubscriptionId"] = "sub",
                ["BanceyBot:Pterodactyl:BaseUrl"] = "https://example.com",
                ["BanceyBot:Pterodactyl:ClientToken"] = "token"
            })
            .Build();
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBanceyBot = new Mock<BanceyBot>(MockBehavior.Loose, mockServiceProvider.Object);
        
        // Act
        var worker = new DiscordWorker(
            logger,
            mockLifetime.Object,
            configuration,
            mockBanceyBot.Object);
        
        // Assert
        Assert.NotNull(worker);
    }
}
