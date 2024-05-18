using System.Text.Json.Serialization;

namespace Bancey.Bot.WorkerService.Model.Pterodactyl;

public class ServerResourcesResponseDto : PterodactylDtoBase
{
  public required ServerResourcesResponseDtoAttributes Attributes { get; set; }
  public class ServerResourcesResponseDtoAttributes
  {
    [JsonPropertyName("current_state")]
    public required string CurrentState { get; set; }
    [JsonPropertyName("is_suspended")]
    public required bool IsSuspended { get; set; }
    public required ServerResourcesResponseResourcesDto Resources { get; set; }
  }
  public class ServerResourcesResponseResourcesDto
  {
    [JsonPropertyName("memory_bytes")]
    public required long MemoryBytes { get; set; }
    [JsonPropertyName("cpu_absolute")]
    public required float CpuAbsolute { get; set; }
    [JsonPropertyName("disk_bytes")]
    public required long DiskBytes { get; set; }
    [JsonPropertyName("network_rx_bytes")]
    public required long NetworkRxBytes { get; set; }
    [JsonPropertyName("network_tx_bytes")]
    public required long NetworkTxBytes { get; set; }
    public required long Uptime { get; set; }
  }
}