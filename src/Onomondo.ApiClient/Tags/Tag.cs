namespace Onomondo.ApiClient.Tags;

public record Tag
{
    public string Id { get; set; } = string.Empty;
    public required string Name { get; set; }
    public bool CanWrite { get; set; }

    /// <summary>
    /// E.g. #f0f0AA
    /// </summary>
    public string? Color { get; set; }
}
