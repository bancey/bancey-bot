using Discord.Interactions;
using Microsoft.ApplicationInsights;

namespace Bancey.Bot.WorkerService;

public abstract class BanceyBotInteractionModuleBase : InteractionModuleBase
{
    internal readonly ILogger<BanceyBotInteractionModuleBase> _logger;
    internal readonly TelemetryClient _telemetryClient;
    internal readonly BanceyBotSettings? _settings;

    public BanceyBotInteractionModuleBase(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient, IConfiguration configuration)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        _settings = configuration.GetRequiredSection("BanceyBot").Get<BanceyBotSettings>();
    }

    public override void BeforeExecute(ICommandInfo command)
    {
        _logger.LogInformation("Executing command: {command}", command.Name);
        base.BeforeExecute(command);
    }

    public override void AfterExecute(ICommandInfo command)
    {
        _logger.LogInformation("Finished executing command: {command}", command.Name);
        base.AfterExecute(command);
    }

    internal async Task<bool> ValidateUser()
    {
        if (_settings == null)
        {
            _logger.LogInformation("Unable to validate user, settings are null.");
            return true;
        }

        bool validChannel = true;
        bool validRoles = true;

        if (_settings.AllowedChannelIds != null && _settings.AllowedChannelIds.Count > 0 && Context.Interaction.ChannelId != null)
        {
            string channelId = Context.Interaction.ChannelId.ToString()!;
            _logger.LogInformation("Checking if channel {channel} is valid...", channelId);
            validChannel = _settings.AllowedChannelIds.Contains(channelId);
        }
        _logger.LogInformation("Channel {channel} is {valid}", Context.Interaction.ChannelId, validChannel ? "valid" : "not valid");

        if (_settings.AllowedRoleIds != null && _settings.AllowedRoleIds.Count > 0)
        {
            _logger.LogInformation("Checking if user has the correct roles...");
            var guildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            validRoles = _settings.AllowedRoleIds.All(allowedRole => guildUser.RoleIds.Contains(ulong.Parse(allowedRole)));
        }
        _logger.LogInformation("User {user} is {valid}", $"{Context.User.GlobalName}-{Context.User.Id}", validRoles);

        return validChannel == true && validRoles == true;
    }
}