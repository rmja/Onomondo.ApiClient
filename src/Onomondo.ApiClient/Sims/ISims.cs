using System.Net;
using Cursor;
using Refit;

namespace Onomondo.ApiClient.Sims;

public interface ISims
{
    [Get("/sims/{simId}")]
    Task<Sim> GetSimByIdAsync(string simId, CancellationToken cancellationToken = default);

    [Get("/sims")]
    [GenerateEnumerator(CursorParameterName = "nextPage")]
    Task<SimPage> ListSimsAsync(
        int? limit = null,
        [AliasAs("next_page")] string? nextPage = null,
        CancellationToken cancellationToken = default
    );

    [Patch("/sims/{simId}")]
    Task<Sim> UpdateSimAsync(
        string simId,
        [Body] SimPatch patch,
        CancellationToken cancellationToken = default
    );

    [Put("/sims/{simId}/tags/{tagId}")]
    Task AddTagAsync(string simId, string tagId, CancellationToken cancellationToken = default);

    [Delete("/sims/{simId}/tags/{tagId}")]
    Task RemoveTagAsync(string simId, string tagId, CancellationToken cancellationToken = default);
}

public static partial class SimsExtensions
{
    public static async Task<bool> SetTagAsync(
        this ISims ops,
        string simId,
        string tagId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await ops.AddTagAsync(simId, tagId, cancellationToken);
            return true;
        }
        catch (ApiException ex)
            when (ex.StatusCode == HttpStatusCode.BadRequest
                && ex.Content == """{"error":"tag_already_on_sim"}"""
            )
        {
            return false;
        }
    }
}
