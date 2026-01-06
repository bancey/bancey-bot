using Bancey.Bot.WorkerService.Model;

namespace Bancey.Bot.Test.Model;

public class BanceyBotSettingsTests
{
    [Fact]
    public void BanceyBotSettings_CanBeCreated()
    {
        // Arrange & Act
        var settings = new BanceyBotSettings
        {
            DiscordToken = "test-token",
            GuildId = 12345,
            AllowedChannelIds = new List<string> { "123", "456" },
            AllowedRoleIds = new List<string> { "role1", "role2" },
            Azure = new BanceyBotAzureSettings
            {
                TenantId = "tenant-id",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                SubscriptionId = "subscription-id",
                Tags = new Dictionary<string, string> { { "env", "test" } }
            },
            Pterodactyl = new BanceyBotPterodactylSettings
            {
                BaseUrl = "https://example.com",
                ClientToken = "token"
            }
        };

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("test-token", settings.DiscordToken);
        Assert.Equal((ulong)12345, settings.GuildId);
        Assert.Equal(2, settings.AllowedChannelIds.Count);
        Assert.Equal(2, settings.AllowedRoleIds.Count);
        Assert.NotNull(settings.Azure);
        Assert.NotNull(settings.Pterodactyl);
    }

    [Fact]
    public void BanceyBotSettings_GuildId_DefaultsToMinValue()
    {
        // Arrange & Act
        var settings = new BanceyBotSettings
        {
            DiscordToken = "test-token",
            AllowedChannelIds = new List<string>(),
            AllowedRoleIds = new List<string>(),
            Azure = new BanceyBotAzureSettings
            {
                TenantId = "tenant-id",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                SubscriptionId = "subscription-id",
                Tags = new Dictionary<string, string>()
            },
            Pterodactyl = new BanceyBotPterodactylSettings
            {
                BaseUrl = "https://example.com",
                ClientToken = "token"
            }
        };

        // Assert
        Assert.Equal(ulong.MinValue, settings.GuildId);
    }
}

public class BanceyBotAzureSettingsTests
{
    [Fact]
    public void BanceyBotAzureSettings_CanBeCreated()
    {
        // Arrange & Act
        var settings = new BanceyBotAzureSettings
        {
            TenantId = "tenant-123",
            ClientId = "client-456",
            ClientSecret = "secret-789",
            SubscriptionId = "sub-012",
            Tags = new Dictionary<string, string>
            {
                { "environment", "production" },
                { "team", "devops" }
            }
        };

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("tenant-123", settings.TenantId);
        Assert.Equal("client-456", settings.ClientId);
        Assert.Equal("secret-789", settings.ClientSecret);
        Assert.Equal("sub-012", settings.SubscriptionId);
        Assert.Equal(2, settings.Tags.Count);
        Assert.True(settings.Tags.ContainsKey("environment"));
    }
}

public class BanceyBotPterodactylSettingsTests
{
    [Fact]
    public void BanceyBotPterodactylSettings_CanBeCreated()
    {
        // Arrange & Act
        var settings = new BanceyBotPterodactylSettings
        {
            BaseUrl = "https://pterodactyl.example.com",
            ClientToken = "ptero-token-123"
        };

        // Assert
        Assert.NotNull(settings);
        Assert.Equal("https://pterodactyl.example.com", settings.BaseUrl);
        Assert.Equal("ptero-token-123", settings.ClientToken);
    }
}
