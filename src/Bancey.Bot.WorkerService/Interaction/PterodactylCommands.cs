using System.Net.Http.Json;
using System.Text;
using Bancey.Bot.WorkerService.Model.Pterodactyl;
using Discord;
using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService.Interaction;

[Group("pterodactyl", "Pterodactyl commands")]
public class PterodactylCommands(ILogger<BanceyBotInteractionModuleBase> logger, TelemetryClient telemetryClient, IConfiguration configuration, HttpClient httpClient) : BanceyBotInteractionModuleBase(logger, telemetryClient, configuration)
{
  private readonly HttpClient _httpClient = httpClient;

  [SlashCommand("list-servers", "Lists the game servers managed by Pterodactyl.")]
  public async Task GetServers()
  {
    var valid = await ValidateUser();
    if (!valid)
    {
      _logger.LogError("User {user} in Channel {channel} is not authorized to run this command.", Context.User.GlobalName, Context.Interaction.ChannelId);
      await RespondAsync("You or this channel are not authorised to run this command. Please contact the bot owner.");
      return;
    }

    var getServersOperation = _telemetryClient.StartOperation<RequestTelemetry>("GetPterodactylServers");
    _logger.LogInformation("Getting servers from Pterodactyl...");
    await DeferAsync();

    try
    {
      using HttpResponseMessage response = await _httpClient.GetAsync("api/client");
      response.EnsureSuccessStatusCode();
      var listResponse = await response.Content.ReadFromJsonAsync<ListResponseDto<ServerResponseDto>>();

      if (listResponse == null)
      {
        _logger.LogError("Error getting servers from Pterodactyl.");
        await FollowupAsync(text: "Error getting servers from Pterodactyl. Please contact the bot owner.");
        _telemetryClient.StopOperation(getServersOperation);
        return;
      }

      var embeds = new List<Embed>();

      foreach (var server in listResponse.Data)
      {
        using HttpResponseMessage resourcesResponse = await _httpClient.GetAsync($"api/client/servers/{server.Attributes.Identifier}/resources");
        response.EnsureSuccessStatusCode();
        var resources = await resourcesResponse.Content.ReadFromJsonAsync<ServerResourcesResponseDto>();

        if (resources == null)
        {
          _logger.LogError("Error getting server resources from Pterodactyl.");
          await FollowupAsync(text: "Error getting server resources from Pterodactyl. Please contact the bot owner.");
          _telemetryClient.StopOperation(getServersOperation);
          continue;
        }

        var description = new StringBuilder();
        description.AppendLine($"**Node**: {server.Attributes.Node}")
          .AppendLine($"**Current State**: {resources.Attributes.CurrentState}");

        if (resources.Attributes.CurrentState == "running")
        {
          description.AppendLine($"**Memory**: {resources.Attributes.Resources.MemoryBytes / 1024 / 1024} MB")
            .AppendLine($"**CPU**: {resources.Attributes.Resources.CpuAbsolute}%")
            .AppendLine($"**Disk**: {resources.Attributes.Resources.DiskBytes / 1024 / 1024} MB")
            .AppendLine($"**Network RX**: {resources.Attributes.Resources.NetworkRxBytes / 1024 / 1024} MB")
            .AppendLine($"**Network TX**: {resources.Attributes.Resources.NetworkTxBytes / 1024 / 1024} MB")
            .AppendLine($"**Uptime**: {resources.Attributes.Resources.Uptime / 1000 / 60} minutes");
        }

        embeds.Add(new EmbedBuilder()
          .WithTitle(server.Attributes.Name)
          .WithDescription(description.ToString())
          .WithCurrentTimestamp()
          .WithColor(resources.Attributes.CurrentState == "running" ? Color.Green : Color.Red).Build());
      }
      await FollowupAsync(embeds: [.. embeds]);
      _telemetryClient.StopOperation(getServersOperation);
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error getting servers from Pterodactyl.");
      await FollowupAsync(text: "Error getting servers from Pterodactyl. Please contact the bot owner.");
      _telemetryClient.StopOperation(getServersOperation);
      return;
    }
  }
}