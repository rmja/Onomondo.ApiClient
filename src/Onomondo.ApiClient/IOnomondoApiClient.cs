namespace Onomondo.ApiClient;

public interface IOnomondoApiClient
{
    public INetworkLogs NetworkLogs { get; }
    public ISims Sims { get; }
    public ITags Tags { get; }
}
