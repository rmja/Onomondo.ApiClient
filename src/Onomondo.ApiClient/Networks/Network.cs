namespace Onomondo.ApiClient.Networks;

public record Network
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public string? Mcc { get; set; }
    public string? Mnc { get; set; }
}
