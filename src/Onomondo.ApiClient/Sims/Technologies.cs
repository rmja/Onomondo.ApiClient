using System.Text.Json.Serialization;

namespace Onomondo.ApiClient.Sims;

public record Technologies
{
    public bool Sms { get; set; }

    [JsonPropertyName("2g_3g")]
    public bool TwoThreeG { get; set; }

    [JsonPropertyName("4g")]
    public bool FourG { get; set; }
}
