using OneOf;
using OneOf.Types;

namespace Onomondo.ApiClient.Sims;

public record SimPatch
{
    /// <summary>
    /// A string alias of the SIM, null if no alias to be assigned.
    /// </summary>
    public OneOf<None, string?> Label { get; set; }

    /// <summary>
    /// Name of the Network Whitelist attached, null if no Network Whitelist should be attached.
    /// </summary>
    public OneOf<None, string?> NetworkWhitelist { get; set; }

    /// <summary>
    /// String IMEI of device the SIM should be locked too (if presented in network signaling) or a null value.
    /// </summary>
    public OneOf<None, string?> ImeiLock { get; set; }

    /// <summary>
    /// Name of Connector attached, set to null if no Connector should be attached.
    /// </summary>
    public OneOf<None, string?> Connector { get; set; }

    /// <summary>
    /// Boolean to activate and deactivate the SIM.
    /// </summary>
    public OneOf<None, bool> Activated { get; set; }

    /// <summary>
    /// Object that can set hard data limits to the SIM.
    /// </summary>
    //public OneOf<None, object?> DataLimit { get; set; }

    /// <summary>
    /// An array of Tag ids to be associated with a SIM. Tag IDs require string format and can be obtained using a GET request on the Tags endpoint /tags.
    /// </summary>
    public OneOf<None, List<string>> Tags { get; set; }

    public OneOf<None, Technologies> Technologies { get; set; }
}
