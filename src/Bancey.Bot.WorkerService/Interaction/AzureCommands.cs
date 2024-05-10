using System.Text;
using Azure;
using Bancey.Bot.WorkerService.Azure;
using Discord;
using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService.Interaction;

[Group("azure", "Azure Commands")]
public class AzureCommands(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient, IConfiguration configuration, AzureClient azureClient) : BanceyBotInteractionModuleBase(logger, telemetryClient, configuration)
{
    private readonly AzureClient _azureClient = azureClient;

    [SlashCommand("list-servers", "Lists servers matching configured tags.")]
    public async Task GetServers()
    {
        if (_settings == null || _settings.Azure == null || _settings.Azure.Tags == null || _settings.Azure.Tags.Count == 0)
        {
            _logger.LogError("Required Azure settings not found.");
            await RespondAsync("Please contact the bot owner, required settings are missing.");
            return;
        }

        var valid = await ValidateUser();
        if (!valid)
        {
            _logger.LogError("User {user} in Channel {channel} is not authorized to run this command.", Context.User.GlobalName, Context.Interaction.ChannelId);
            await RespondAsync("You or this channel are not authorised to run this command. Please contact the bot owner.");
            return;
        }

        var formattedTags = _settings.Azure.Tags.Select(tag => $"{tag.Key}={tag.Value}");

        var getServersOperation = _telemetryClient.StartOperation<RequestTelemetry>("GetServers");
        _logger.LogInformation("Getting servers matching configured tags. {tags}", formattedTags);
        await DeferAsync();
        var subscription = await _azureClient.GetSubscription();
        var virtualMachines = await _azureClient.GetVirtualMachines(subscription, _settings.Azure.Tags);

        var embed = new EmbedBuilder()
          .WithTitle("Azure Servers")
          .WithColor(Color.DarkTeal);

        var stringBuilder = new StringBuilder();
        foreach (var virtualMachine in virtualMachines)
        {
            stringBuilder
              .AppendLine($"### {virtualMachine.Data.Name}")
              .AppendLine($"**Status**: `{virtualMachine.Get().Value.InstanceView().Value.Statuses[1].DisplayStatus}`");
        }
        stringBuilder.AppendLine();
        var commandId = (await Context.Guild.GetApplicationCommandsAsync()).FirstOrDefault(cmd => cmd.Name == "azure")?.Id;
        stringBuilder.AppendLine($"Start a server: </azure start-server:{commandId}>");

        embed.WithDescription(stringBuilder.ToString()).WithFooter($"Total: {virtualMachines.Count}, Tags: {string.Join(", ", formattedTags)}").WithCurrentTimestamp();
        await FollowupAsync(embeds: [embed.Build()]);
        _telemetryClient.StopOperation(getServersOperation);
    }

    [SlashCommand("start-server", "Starts a specific Azure VM.")]
    public async Task StartServer([Summary("serverName", "Attempts to start a server in Azure."), Autocomplete(typeof(VirtualMachineAutoCompleteHandler))] string serverName)
    {
        var valid = await ValidateUser();
        if (!valid)
        {
            _logger.LogError("User {user} in Channel {channel} is not authorized to run this command.", Context.User.GlobalName, Context.Interaction.ChannelId);
            await RespondAsync("You or this channel are not authorised to run this command. Please contact the bot owner.");
            return;
        }
        var startServerOperation = _telemetryClient.StartOperation<RequestTelemetry>("StartServer");
        _logger.LogInformation("Starting server {serverName}", serverName);
        await DeferAsync();
        var subscription = await _azureClient.GetSubscription();
        var virtualMachine = await _azureClient.GetVirtualMachineAsync(subscription, serverName);

        if (virtualMachine == null)
        {
            await FollowupAsync($"No server found with the name {serverName}.");
            _telemetryClient.StopOperation(startServerOperation);
            return;
        }

        var result = await virtualMachine.PowerOnAsync(WaitUntil.Started);
        _logger.LogInformation("Server {serverName} starting...", serverName);
        await FollowupAsync($"Server {serverName} starting up...");

        while (result.HasCompleted == false)
        {
            _logger.LogInformation("Waiting for server {serverName} to start...", serverName);
            await Task.Delay(5000);
            await result.UpdateStatusAsync();
        }

        _logger.LogInformation("Server {serverName} started.", serverName);
        await FollowupAsync($"Server {serverName} started.");
        _telemetryClient.StopOperation(startServerOperation);
    }

    [SlashCommand("stop-server", "Stops a specific Azure VM.")]
    public async Task StopServer([Summary("serverName", "Attempts to stop a server in Azure."), Autocomplete(typeof(VirtualMachineAutoCompleteHandler))] string serverName)
    {
        var valid = await ValidateUser();
        if (!valid)
        {
            _logger.LogError("User {user} in Channel {channel} is not authorized to run this command.", Context.User.GlobalName, Context.Interaction.ChannelId);
            await RespondAsync("You or this channel are not authorised to run this command. Please contact the bot owner.");
            return;
        }

        var stopServerOperation = _telemetryClient.StartOperation<RequestTelemetry>("StopServer");
        _logger.LogInformation("Shutting down server {serverName} gracefully.", serverName);
        await DeferAsync();
        var subscription = await _azureClient.GetSubscription();
        var virtualMachine = await _azureClient.GetVirtualMachineAsync(subscription, serverName);

        if (virtualMachine == null)
        {
            await FollowupAsync($"No server found with the name {serverName}.");
            _telemetryClient.StopOperation(stopServerOperation);
            return;
        }

        var result = await virtualMachine.PowerOffAsync(WaitUntil.Started, false);
        _logger.LogInformation("Server {serverName} shutting down...", serverName);
        await FollowupAsync($"Server {serverName} shutting down...");

        await result.UpdateStatusAsync();

        while (result.HasCompleted == false)
        {
            _logger.LogInformation("Waiting for server {serverName} to shutdown...", serverName);
            await Task.Delay(5000);
            await result.UpdateStatusAsync();
        }

        _logger.LogInformation("Server {serverName} shutdown.", serverName);
        await FollowupAsync($"Server {serverName} shutdown... Now deallocating resources.");

        var deallocateResult = await virtualMachine.DeallocateAsync(WaitUntil.Started);
        _logger.LogInformation("Server {serverName} deallocating.", serverName);

        while (deallocateResult.HasCompleted == false)
        {
            _logger.LogInformation("Waiting for server {serverName} to deallocate...", serverName);
            await Task.Delay(5000);
            await deallocateResult.UpdateStatusAsync();
        }

        _logger.LogInformation("Server {serverName} deallocated.", serverName);
        await FollowupAsync($"Server {serverName} deallocated.");
        _telemetryClient.StopOperation(stopServerOperation);
    }
}