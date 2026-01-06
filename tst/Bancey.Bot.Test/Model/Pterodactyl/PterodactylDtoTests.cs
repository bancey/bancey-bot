using Bancey.Bot.WorkerService.Model.Pterodactyl;

namespace Bancey.Bot.Test.Model.Pterodactyl;

public class PterodactylDtoTests
{
    [Fact]
    public void PterodactylDtoBase_CanBeCreated()
    {
        // Arrange & Act
        var dto = new PterodactylDtoBase
        {
            ObjectType = "server"
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("server", dto.ObjectType);
    }

    [Fact]
    public void ServerResponseDto_CanBeCreated()
    {
        // Arrange & Act
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

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("server", dto.ObjectType);
        Assert.NotNull(dto.Attributes);
        Assert.Equal("Test Server", dto.Attributes.Name);
        Assert.Equal("test123", dto.Attributes.Identifier);
        Assert.Equal("uuid-123-456", dto.Attributes.Uuid);
        Assert.Equal("node1", dto.Attributes.Node);
    }

    [Fact]
    public void ListResponseDto_CanBeCreated()
    {
        // Arrange & Act
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
                }
            }
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("list", dto.ObjectType);
        Assert.NotNull(dto.Data);
        Assert.Single(dto.Data);
        Assert.Equal("Server 1", dto.Data[0].Attributes.Name);
    }

    [Fact]
    public void ServerResourcesResponseDto_CanBeCreated()
    {
        // Arrange & Act
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

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("stats", dto.ObjectType);
        Assert.NotNull(dto.Attributes);
        Assert.Equal("running", dto.Attributes.CurrentState);
        Assert.False(dto.Attributes.IsSuspended);
        Assert.NotNull(dto.Attributes.Resources);
        Assert.Equal(1024000, dto.Attributes.Resources.MemoryBytes);
        Assert.Equal(50.5f, dto.Attributes.Resources.CpuAbsolute);
        Assert.Equal(2048000, dto.Attributes.Resources.DiskBytes);
        Assert.Equal(512000, dto.Attributes.Resources.NetworkRxBytes);
        Assert.Equal(256000, dto.Attributes.Resources.NetworkTxBytes);
        Assert.Equal(3600, dto.Attributes.Resources.Uptime);
    }

    [Fact]
    public void ServerResourcesResponseDto_CanHandleSuspendedServer()
    {
        // Arrange & Act
        var dto = new ServerResourcesResponseDto
        {
            ObjectType = "stats",
            Attributes = new ServerResourcesResponseDto.ServerResourcesResponseDtoAttributes
            {
                CurrentState = "stopped",
                IsSuspended = true,
                Resources = new ServerResourcesResponseDto.ServerResourcesResponseResourcesDto
                {
                    MemoryBytes = 0,
                    CpuAbsolute = 0f,
                    DiskBytes = 0,
                    NetworkRxBytes = 0,
                    NetworkTxBytes = 0,
                    Uptime = 0
                }
            }
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("stopped", dto.Attributes.CurrentState);
        Assert.True(dto.Attributes.IsSuspended);
        Assert.Equal(0, dto.Attributes.Resources.Uptime);
    }
}
