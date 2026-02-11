using Onomondo.ApiClient.Tags;

namespace Onomondo.ApiClient.Sims;

public record Sim
{
    public required string Id { get; set; }
    public required string Iccid { get; set; }
    public string? Label { get; set; }
    public string? NetworkWhitelist { get; set; }
    public string? ImeiLock { get; set; }
    public string? Imei { get; set; }
    public string? Imsi { get; set; }
    public string? Connector { get; set; }
    public bool Activated { get; set; }
    public required string Ipv4 { get; set; }
    public bool Online { get; set; }
    public DateTime? OnlineAt { get; set; }
    public DateTime? LastCameOnlineAt { get; set; }
    public Network Network { get; set; } = new();
    public long Usage { get; set; }
    public List<Tag> Tags { get; set; } = [];
    public Technologies Technologies { get; set; } = new();
}
