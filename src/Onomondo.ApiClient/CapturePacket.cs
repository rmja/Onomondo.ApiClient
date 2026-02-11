using System.Net;

namespace Onomondo.ApiClient;

public record CapturePacket(byte[] PacketBytes)
{
    public required string SimId { get; init; }
    public required IPAddress SimIp { get; init; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
