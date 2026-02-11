using Cursor;

namespace Onomondo.ApiClient.Sims;

public record SimPage : ICursorPage<Sim>
{
    public Pagination? Pagination { get; set; }
    public List<Sim> Sims { get; set; } = [];

    List<Sim> ICursorPage<Sim>.Items => Sims;

    string? ICursorPage<Sim>.NextCursor => Pagination?.NextPage;
}
