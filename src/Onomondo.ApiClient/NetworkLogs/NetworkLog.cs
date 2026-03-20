using Onomondo.ApiClient.Networks;

namespace Onomondo.ApiClient.NetworkLogs;

public record NetworkLog
{
    public Guid Id { get; init; }
    public DateTimeOffset Time { get; init; }
    public required string SimId { get; init; }
    public required string Iccid { get; init; }
    public required string Imei { get; init; }
    public string? Status { get; init; }
    public string? Reason { get; init; }
    public required string NetworkType { get; init; }
    public required Network Network { get; init; }
    public LogType LogType { get; init; }
}
