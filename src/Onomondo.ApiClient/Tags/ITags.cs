using Cursor;
using Onomondo.ApiClient.Tags;
using Refit;

namespace Onomondo.ApiClient;

public interface ITags
{
    [Get("/tags/search/tags")]
    [GenerateEnumerator(CursorParameterName = "offset")]
    Task<TagPage> ListTagsAsync(
        int offset = 0,
        int? limit = null,
        CancellationToken cancellationToken = default
    );
}
