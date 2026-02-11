using System.Runtime.CompilerServices;
using Cursor;
using Refit;

namespace Onomondo.ApiClient.Tags;

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
