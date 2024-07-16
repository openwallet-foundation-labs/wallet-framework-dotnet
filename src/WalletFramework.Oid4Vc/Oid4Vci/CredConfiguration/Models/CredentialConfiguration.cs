using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Format;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Scope;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CryptographicBindingMethod;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CryptograhicSigningAlgValue;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.ProofTypeId;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.ProofTypeMetadata;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialDisplay;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents the metadata of a specific type of credential that a Credential Issuer can issue.
/// </summary>
public record CredentialConfiguration
{
    /// <summary>
    ///     Gets the identifier for the format of the credential.
    /// </summary>
    [JsonProperty(FormatJsonKey)]
    public Format Format { get; }
        
    /// <summary>
    ///     Gets a string indicating the credential that can be issued.
    /// </summary>
    [JsonProperty(ScopeJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<Scope>))]
    public Option<Scope> Scope { get; set; }

    /// <summary>
    ///     Gets list of methods that identify how the Credential is bound to the identifier of the End-User who
    ///     possesses the Credential.
    /// </summary>
    [JsonProperty(CryptographicBindingMethodsSupportedJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<List<CryptographicBindingMethod>>))]
    public Option<List<CryptographicBindingMethod>> CryptographicBindingMethodsSupported { get; }

    /// <summary>
    ///     Gets a list of identifiers for the signing algorithms that are supported by the issuer and used
    ///     to sign credentials.
    /// </summary>
    [JsonProperty(CredentialSigningAlgValuesSupportedJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<List<CryptograhicSigningAlgValue>>))]
    public Option<List<CryptograhicSigningAlgValue>> CredentialSigningAlgValuesSupported { get; }
        
    /// <summary>
    ///     Gets a dictionary which maps a credential type to its supported signing algorithms for key proofs.
    /// </summary>
    [JsonProperty(ProofTypesSupportedJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<Dictionary<ProofTypeId, ProofTypeMetadata>>))]
    public Option<Dictionary<ProofTypeId, ProofTypeMetadata>> ProofTypesSupported { get; }

    /// <summary>
    ///     Gets a list of display properties of the supported credential for different languages.
    /// </summary>
    [JsonProperty(DisplayJsonKey)]
    [JsonConverter(typeof(OptionJsonConverter<List<CredentialDisplay>>))]
    public Option<List<CredentialDisplay>> Display { get; }

    private CredentialConfiguration(
        Format format,
        Option<Scope> scope,
        Option<List<CryptographicBindingMethod>> cryptographicBindingMethodsSupported,
        Option<List<CryptograhicSigningAlgValue>> credentialSigningAlgValuesSupported,
        Option<Dictionary<ProofTypeId, ProofTypeMetadata>> proofTypesSupported,
        Option<List<CredentialDisplay>> display)
    {
        Format = format;
        Scope = scope;
        CryptographicBindingMethodsSupported = cryptographicBindingMethodsSupported;
        CredentialSigningAlgValuesSupported = credentialSigningAlgValuesSupported;
        ProofTypesSupported = proofTypesSupported;
        Display = display;
    }
    
    private static CredentialConfiguration Create(
        Format format,
        Option<Scope> scope,
        Option<List<CryptographicBindingMethod>> cryptographicBindingMethodsSupported,
        Option<List<CryptograhicSigningAlgValue>> credentialSigningAlgValuesSupported,
        Option<Dictionary<ProofTypeId, ProofTypeMetadata>> proofTypesSupported,
        Option<List<CredentialDisplay>> display) => new(
        format,
        scope,
        cryptographicBindingMethodsSupported,
        credentialSigningAlgValuesSupported,
        proofTypesSupported,
        display);

    public static Validation<CredentialConfiguration> ValidCredentialConfiguration(JToken credentialMetadata)
    {
        var validBindingMethods = new Func<JToken, Validation<List<CryptographicBindingMethod>>>(bindingMethods => 
            from array in bindingMethods.ToJArray()
            from methods in array.TraverseAny(ValidCryptographicBindingMethod)
            select methods.ToList());
        
        var validSigningAlgValues = new Func<JToken, Validation<List<CryptograhicSigningAlgValue>>>(signingAlgValues => 
            from array in signingAlgValues.ToJArray()
            from values in array.TraverseAny(ValidCryptograhicSigningAlgValue)
            select values.ToList());
        
        var validProofTypes = new Func<JToken, Validation<Dictionary<ProofTypeId, ProofTypeMetadata>>>(proofTypes => proofTypes
            .ToJObject()
            .OnSuccess(jObject => jObject.ToValidDictionaryAny(ValidProofTypeId, ValidProofTypeMetadata)));
        
        var optionalCredentialDisplays = new Func<JToken, Option<List<CredentialDisplay>>>(credentialDisplays => 
            from array in credentialDisplays.ToJArray().ToOption()
            from displays in array.TraverseAny(OptionalCredentialDisplay)
            select displays.ToList());
        
        return Valid(Create)
            .Apply(credentialMetadata.GetByKey(FormatJsonKey).OnSuccess(ValidFormat))
            .Apply(credentialMetadata.GetByKey(ScopeJsonKey).ToOption().OnSome(OptionalScope))
            .Apply(credentialMetadata.GetByKey(CryptographicBindingMethodsSupportedJsonKey).OnSuccess(validBindingMethods).ToOption())
            .Apply(credentialMetadata.GetByKey(CredentialSigningAlgValuesSupportedJsonKey).OnSuccess(validSigningAlgValues).ToOption())
            .Apply(credentialMetadata.GetByKey(ProofTypesSupportedJsonKey).OnSuccess(validProofTypes).ToOption())
            .Apply(credentialMetadata.GetByKey(DisplayJsonKey).ToOption().OnSome(optionalCredentialDisplays));
    }

    private const string FormatJsonKey = "format";
    private const string ScopeJsonKey = "scope";
    private const string CryptographicBindingMethodsSupportedJsonKey = "cryptographic_binding_methods_supported";
    private const string CredentialSigningAlgValuesSupportedJsonKey = "credential_signing_alg_values_supported";
    private const string ProofTypesSupportedJsonKey = "proof_types_supported";
    private const string DisplayJsonKey = "display";
}
