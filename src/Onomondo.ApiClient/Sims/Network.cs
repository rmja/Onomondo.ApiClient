namespace Onomondo.ApiClient.Sims;

public record Network
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Mcc { get; set; }
    public string? Mnc { get; set; }
}
