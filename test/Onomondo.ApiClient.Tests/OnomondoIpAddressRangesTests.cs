using System.Net;

namespace Onomondo.ApiClient.Tests;

public class OnomondoIpAddressRangesTests
{
    [Theory]
    [InlineData("3.69.121.60")]
    [InlineData("3.64.84.111")]
    public void IsSimIp(string ip)
    {
        Assert.True(OnomondoAddressRanges.IsSimAddress(IPAddress.Parse(ip)));
    }
}
