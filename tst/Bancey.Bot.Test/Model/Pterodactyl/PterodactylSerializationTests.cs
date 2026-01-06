using System.Text.Json;
using Bancey.Bot.WorkerService.Model.Pterodactyl;

namespace Bancey.Bot.Test.Model.Pterodactyl;

public class PterodactylSerializationTests
{
    [Fact]
    public void ServerResponseDto_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var dto = new ServerResponseDto
        {
            ObjectType = "server",
            Attributes = new ServerResponseDto.ServerResponseDtoAttributes
            {
                Name = "Test Server",
                Identifier = "test123",
                Uuid = "uuid-123-456",
                Node = "node1"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ServerResponseDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.ObjectType, deserialized.ObjectType);
        Assert.Equal(dto.Attributes.Name, deserialized.Attributes.Name);
        Assert.Equal(dto.Attributes.Identifier, deserialized.Attributes.Identifier);
        Assert.Equal(dto.Attributes.Uuid, deserialized.Attributes.Uuid);
        Assert.Equal(dto.Attributes.Node, deserialized.Attributes.Node);
    }

    [Fact]
    public void ServerResourcesResponseDto_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var dto = new ServerResourcesResponseDto
        {
            ObjectType = "stats",
            Attributes = new ServerResourcesResponseDto.ServerResourcesResponseDtoAttributes
            {
                CurrentState = "running",
                IsSuspended = false,
                Resources = new ServerResourcesResponseDto.ServerResourcesResponseResourcesDto
                {
                    MemoryBytes = 1024000,
                    CpuAbsolute = 50.5f,
                    DiskBytes = 2048000,
                    NetworkRxBytes = 512000,
                    NetworkTxBytes = 256000,
                    Uptime = 3600
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ServerResourcesResponseDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.ObjectType, deserialized.ObjectType);
        Assert.Equal(dto.Attributes.CurrentState, deserialized.Attributes.CurrentState);
        Assert.Equal(dto.Attributes.IsSuspended, deserialized.Attributes.IsSuspended);
        Assert.Equal(dto.Attributes.Resources.MemoryBytes, deserialized.Attributes.Resources.MemoryBytes);
        Assert.Equal(dto.Attributes.Resources.CpuAbsolute, deserialized.Attributes.Resources.CpuAbsolute);
    }

    [Fact]
    public void ListResponseDto_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var dto = new ListResponseDto<ServerResponseDto>
        {
            ObjectType = "list",
            Data = new List<ServerResponseDto>
            {
                new ServerResponseDto
                {
                    ObjectType = "server",
                    Attributes = new ServerResponseDto.ServerResponseDtoAttributes
                    {
                        Name = "Server 1",
                        Identifier = "server1",
                        Uuid = "uuid-1",
                        Node = "node1"
                    }
                },
                new ServerResponseDto
                {
                    ObjectType = "server",
                    Attributes = new ServerResponseDto.ServerResponseDtoAttributes
                    {
                        Name = "Server 2",
                        Identifier = "server2",
                        Uuid = "uuid-2",
                        Node = "node2"
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ListResponseDto<ServerResponseDto>>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(dto.ObjectType, deserialized.ObjectType);
        Assert.Equal(2, deserialized.Data.Count);
        Assert.Equal("Server 1", deserialized.Data[0].Attributes.Name);
        Assert.Equal("Server 2", deserialized.Data[1].Attributes.Name);
    }

    [Fact]
    public void ServerResourcesResponseDto_DeserializesJsonPropertyNames()
    {
        // Arrange
        var json = @"{
            ""object"": ""stats"",
            ""attributes"": {
                ""current_state"": ""running"",
                ""is_suspended"": false,
                ""resources"": {
                    ""memory_bytes"": 1024000,
                    ""cpu_absolute"": 50.5,
                    ""disk_bytes"": 2048000,
                    ""network_rx_bytes"": 512000,
                    ""network_tx_bytes"": 256000,
                    ""uptime"": 3600
                }
            }
        }";

        // Act
        var deserialized = JsonSerializer.Deserialize<ServerResourcesResponseDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("stats", deserialized.ObjectType);
        Assert.Equal("running", deserialized.Attributes.CurrentState);
        Assert.False(deserialized.Attributes.IsSuspended);
        Assert.Equal(1024000, deserialized.Attributes.Resources.MemoryBytes);
        Assert.Equal(50.5f, deserialized.Attributes.Resources.CpuAbsolute);
        Assert.Equal(3600, deserialized.Attributes.Resources.Uptime);
    }
}
