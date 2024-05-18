using System.Text.Json.Serialization;

namespace Bancey.Bot.WorkerService.Model.Pterodactyl
{
  public class PterodactylDtoBase
  {
    [JsonPropertyName("object")]
    public required string ObjectType { get; set; }
  }
}