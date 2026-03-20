using Cursor;
using Onomondo.ApiClient.NetworkLogs;
using Refit;

namespace Onomondo.ApiClient;

public interface INetworkLogs
{
    [Get("/network-logs/{simId}")]
    [GenerateEnumerator(CursorParameterName = "offset")]
    Task<NetworkLogPage> ListSimNetworkLogsAsync(
        string simId,
        DateTimeOffset timestamp,
        int offset = 0,
        int? limit = null,
        CancellationToken cancellationToken = default
    );
}
