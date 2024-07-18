using Newtonsoft.Json.Linq;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vci.CredOffer.Samples;

public static class CredentialOfferSample
{
    public const string CredentialIssuer = "https://credential-issuer.example.com";
    
    public const string UniversityDegreeCredential = "UniversityDegreeCredential";
    
    public const string OrgIso1801351Mdl = "org.iso.18013.5.1.mDL";

    public const string PreAuthorizedCode = "oaKazRN8I0IbtZ0C7JuMn5";
    
    public const int Length = 4;
    
    public const string Description = "Please provide the one-time code that was sent via e-mail";
    
    public const string InputMode = "numeric";
    
    public static JObject PreAuth => new()
    {
        ["credential_issuer"] = CredentialIssuer,
        ["credential_configuration_ids"] = new JArray
        {
            UniversityDegreeCredential,
            OrgIso1801351Mdl
        },
        ["grants"] = new JObject
        {
            ["urn:ietf:params:oauth:grant-type:pre-authorized_code"] = new JObject
            {
                ["pre-authorized_code"] = PreAuthorizedCode,
                ["tx_code"] = new JObject
                {
                    ["length"] = Length,
                    ["input_mode"] = InputMode,
                    ["description"] = Description
                }
            }
        }
    };
}
