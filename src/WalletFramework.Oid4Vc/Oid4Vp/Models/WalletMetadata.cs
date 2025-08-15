using Newtonsoft.Json.Linq;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record WalletMetadata
{
    private const string VpFormatsSupportedIdentifier = "vp_formats_supported";
    private const string ClientIdPrefixesSupportedIdentifier = "client_id_prefixes_supported";
    //TODO: Remove the following identifier in the future, it is deprecated but kept for backwards compatibility for now.
    private const string ClientIdSchemesSupportedIdentifier = "client_id_schemes_supported";
    
    public Formats VpFormatsSupported { get; }

    public ClientIdScheme[] ClientIdPrefixesSupported { get; }

    private  WalletMetadata(Formats vpFormatsSupported, ClientIdScheme[] clientIdPrefixesSupported)
    {
        VpFormatsSupported = vpFormatsSupported;
        ClientIdPrefixesSupported = clientIdPrefixesSupported;
    }

    public static WalletMetadata CreateDefault()
    {
        var vpFormatsSupported = new Formats
        {
            SdJwtVcFormat = new SdJwtFormat
            {
                IssuerSignedJwtAlgValues = ["ES256", "ES384", "ES512", "RS256"],
                KeyBindingJwtAlgValues = ["ES256"]
            },
            SdJwtDcFormat = new SdJwtFormat
            {
                IssuerSignedJwtAlgValues = ["ES256", "ES384", "ES512", "RS256"],
                KeyBindingJwtAlgValues = ["ES256"]
            },
            MDocFormat = new MDocFormat
            {
                IssuerAuthAlgValues = ["-7", "-35", "-36", "-8"],
                DeviceAuthAlgValues = ["-7"]
            }
        };

        var clientIdPrefixesSupported = new []{(ClientIdScheme)ClientIdScheme.RedirectUriScheme, (ClientIdScheme)ClientIdScheme.X509SanDnsScheme};

        return new WalletMetadata(vpFormatsSupported, clientIdPrefixesSupported);
    }
    
    public string ToJsonString()
    {
        return new JObject
        {
            [VpFormatsSupportedIdentifier] = JObject.FromObject(VpFormatsSupported),
            [ClientIdPrefixesSupportedIdentifier] = new JArray {ClientIdPrefixesSupported.Select(x => x.AsString())},
            [ClientIdSchemesSupportedIdentifier] = new JArray {ClientIdPrefixesSupported.Select(x => x.AsString())},
        }.ToString();
    }
} 
