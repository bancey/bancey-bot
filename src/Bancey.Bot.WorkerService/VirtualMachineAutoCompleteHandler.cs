using Azure.ResourceManager.Compute;
using Discord;
using Discord.Interactions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Bancey.Bot.WorkerService;

public class VirtualMachineAutoCompleteHandler(ILogger<VirtualMachineAutoCompleteHandler> logger, TelemetryClient telemetryClient, ResourceCacheManager<VirtualMachineResource> vmCache) : AutocompleteHandler
{
  private ILogger<VirtualMachineAutoCompleteHandler> _logger = logger;
  private readonly TelemetryClient _telemetryClient = telemetryClient;
  private ResourceCacheManager<VirtualMachineResource> _vmCache = vmCache;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
  public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
  {
    var virtualMachineAutoCompleteOperation = _telemetryClient.StartOperation<RequestTelemetry>("VirtualMachineAutoComplete");
    List<AutocompleteResult> results = new List<AutocompleteResult>();
    foreach (var key in _vmCache.CachedKeys())
    {
      results.Add(new AutocompleteResult(key, key));
    }
    _logger.LogInformation("Generated {count} suggestions.", results.Count);
    _telemetryClient.StopOperation(virtualMachineAutoCompleteOperation);
    return AutocompletionResult.FromSuccess(results.Take(25));
  }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}