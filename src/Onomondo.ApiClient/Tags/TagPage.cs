using Cursor;

namespace Onomondo.ApiClient.Tags;

public record TagPage : ICursorPage<Tag>
{
    public int Count { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int Total { get; set; }
    public List<Tag> Result { get; set; } = [];

    List<Tag> ICursorPage<Tag>.Items => Result;

    string? ICursorPage<Tag>.NextCursor => null;

    bool ICursorPage<Tag>.HasMore => Offset + Result.Count < Total;
}
