using System.Net;
using NetTools;

namespace Onomondo.ApiClient;

// See docs.onomondo.com
public static class OnomondoAddressRanges
{
    public static IEnumerable<IPAddressRange> SimSubnets { get; } =
        new[]
        {
            "158.177.93.16/28",
            "185.228.69.0/24",
            "185.228.70.0/24",
            "3.69.121.60/32",
            "3.69.192.150/32",
            "3.64.84.111/32",
        }
            .Select(IPAddressRange.Parse)
            .ToArray();

    public static IEnumerable<IPAddressRange> WebhookSubnets { get; } =
        new[] { "3.65.45.209/32", "35.158.167.193/32", "52.58.186.11/32" }
            .Select(IPAddressRange.Parse)
            .ToArray();

    public static bool IsSimAddress(IPAddress address) => SimSubnets.Any(x => x.Contains(address));

    public static bool IsWebhookAddress(IPAddress address) =>
        WebhookSubnets.Any(x => x.Contains(address));
}
