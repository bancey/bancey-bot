namespace Bancey.Bot.WorkerService.Model.Pterodactyl;

public sealed class ListResponseDto<T> : PterodactylDtoBase
{
  public required List<T> Data { get; set; }
}