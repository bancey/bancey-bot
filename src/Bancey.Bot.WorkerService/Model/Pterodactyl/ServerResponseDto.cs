using System.Text.Json.Serialization;

namespace Bancey.Bot.WorkerService.Model.Pterodactyl;

public class ServerResponseDto : PterodactylDtoBase
{
  public required ServerResponseDtoAttributes Attributes { get; set; }
  public class ServerResponseDtoAttributes
  {
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public required string Uuid { get; set; }
    public required string Node { get; set; }
  }
}