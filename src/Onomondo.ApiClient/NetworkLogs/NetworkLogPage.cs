using Cursor;

namespace Onomondo.ApiClient.NetworkLogs;

public record NetworkLogPage : ICursorPage<NetworkLog>
{
    public List<NetworkLog> Docs { get; set; } = [];

    List<NetworkLog> ICursorPage<NetworkLog>.Items => Docs;

    string? ICursorPage<NetworkLog>.NextCursor
    {
        get
        {
            var count = Docs.Count;
            return count > 0 ? count.ToString() : null;
        }
    }
}
