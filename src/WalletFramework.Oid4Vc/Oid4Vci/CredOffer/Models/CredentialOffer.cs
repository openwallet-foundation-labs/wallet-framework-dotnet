using System.Globalization;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models.CredentialConfigurationId;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models.Grants;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

/// <summary>
///     Represents an OpenID4VCI Credential Offer, which is used to obtain one or more credentials from a Credential
///     Issuer.
/// </summary>
public record CredentialOffer
{
    /// <summary>
    ///     Gets the URL of the Credential Issuer from where the Wallet is requested to obtain one or more Credentials
    ///     from.
    /// </summary>
    [JsonProperty("credential_issuer")]
    public Uri CredentialIssuer { get; }
    
    /// <summary>
    ///     Gets the list of credentials that the Wallet may request. The List contains CredentialMetadataIds
    ///     that must map to the keys in the credential_configurations_supported dictionary of the Issuer Metadata
    /// </summary>
    [JsonProperty("credential_configuration_ids")]
    public List<CredentialConfigurationId> CredentialConfigurationIds { get; }

    /// <summary>
    ///     Gets the JSON object indicating to the Wallet the Grant Types the Credential Issuer's Authorization Server
    ///     is prepared to process for this credential offer. If not present or empty, the Wallet must determine the
    ///     Grant Types the Credential Issuer's AS supports using the respective metadata.
    /// </summary>
    [JsonProperty("grants")]
    public Option<Grants> Grants { get; }

    private CredentialOffer(
        Uri credentialIssuer,
        List<CredentialConfigurationId> credentialConfigurationIds,
        Option<Grants> grants) 
    {
        CredentialIssuer = credentialIssuer;
        CredentialConfigurationIds = credentialConfigurationIds;
        Grants = grants;
    }

    private static CredentialOffer Create(
        Uri credentialIssuer,
        List<CredentialConfigurationId> credentialConfigurationIds,
        Option<Grants> grants) => new(credentialIssuer, credentialConfigurationIds, grants);

    public static Validation<CredentialOffer> ValidCredentialOffer(JObject json)
    {
        var validCredentialIssuer = new Func<JToken, Validation<Uri>>(token => token.ToJValue().OnSuccess(value =>
        {
            try
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                return new Uri(str);
            }
            catch (Exception e)
            {
                return new CredentialIssuerError(value, e).ToInvalid<Uri>();
            }
        }));

        var validCredentialConfigurationsIds = new Func<JToken, Validation<List<CredentialConfigurationId>>>(token => 
            from jArray in token.ToJArray()
            from configurationIds in jArray.TraverseAny(ValidCredentialConfigurationId)
            select configurationIds.ToList());

        return Valid(Create)
            .Apply(json.GetByKey("credential_issuer").OnSuccess(validCredentialIssuer))
            .Apply(json.GetByKey("credential_configuration_ids").OnSuccess(validCredentialConfigurationsIds))
            .Apply(json.GetByKey("grants").OnSuccess(OptionalGrants));
    }
}
