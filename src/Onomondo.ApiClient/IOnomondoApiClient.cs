namespace Onomondo.ApiClient;

public interface IOnomondoApiClient
{
    public ISims Sims { get; }
    public ITags Tags { get; }
}
