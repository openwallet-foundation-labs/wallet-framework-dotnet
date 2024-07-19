using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Format;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Scope;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CryptographicBindingMethod;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CryptograhicSigningAlgValue;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.ProofTypeId;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.ProofTypeMetadata;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialDisplay;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialConfigurationFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents the metadata of a specific type of credential that a Credential Issuer can issue.
/// </summary>
public record CredentialConfiguration
{
    /// <summary>
    ///     Gets the identifier for the format of the credential.
    /// </summary>
    public Format Format { get; }
        
    /// <summary>
    ///     Gets a string indicating the credential that can be issued.
    /// </summary>
    public Option<Scope> Scope { get; set; }

    /// <summary>
    ///     Gets list of methods that identify how the Credential is bound to the identifier of the End-User who
    ///     possesses the Credential.
    /// </summary>
    public Option<List<CryptographicBindingMethod>> CryptographicBindingMethodsSupported { get; }

    /// <summary>
    ///     Gets a list of identifiers for the signing algorithms that are supported by the issuer and used
    ///     to sign credentials.
    /// </summary>
    public Option<List<CryptograhicSigningAlgValue>> CredentialSigningAlgValuesSupported { get; }
        
    /// <summary>
    ///     Gets a dictionary which maps a credential type to its supported signing algorithms for key proofs.
    /// </summary>
    public Option<Dictionary<ProofTypeId, ProofTypeMetadata>> ProofTypesSupported { get; }

    /// <summary>
    ///     Gets a list of display properties of the supported credential for different languages.
    /// </summary>
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
}

public static class CredentialConfigurationFun
{
    public const string FormatJsonKey = "format";
    public const string ScopeJsonKey = "scope";
    public const string CryptographicBindingMethodsSupportedJsonKey = "cryptographic_binding_methods_supported";
    public const string CredentialSigningAlgValuesSupportedJsonKey = "credential_signing_alg_values_supported";
    public const string ProofTypesSupportedJsonKey = "proof_types_supported";
    public const string DisplayJsonKey = "display";
    
    public static JObject EncodeToJson(this CredentialConfiguration config)
    {
        var result = new JObject();

        result.Add(FormatJsonKey, config.Format.ToString());
        
        config.Scope.IfSome(scope => result.Add(ScopeJsonKey, scope.ToString()));

        config.CryptographicBindingMethodsSupported.IfSome(list =>
        {
            var bindingMethods = new JArray();
            foreach (var method in list)
            {
                bindingMethods.Add(method.ToString());
            }
            result.Add(CryptographicBindingMethodsSupportedJsonKey, bindingMethods);
        });
        
        config.CredentialSigningAlgValuesSupported.IfSome(list =>
        {
            var signingAlgValues = new JArray();
            foreach (var value in list)
            {
                signingAlgValues.Add(value.ToString());
            }
            result.Add(CredentialSigningAlgValuesSupportedJsonKey, signingAlgValues);
        });
        
        config.ProofTypesSupported.IfSome(dict =>
        {
            var proofTypes = new JObject();
            foreach (var (key, value) in dict)
            {
                proofTypes.Add(key.ToString(), value.EncodeToJson());
            }
            result.Add(ProofTypesSupportedJsonKey, proofTypes);
        });
        
        config.Display.IfSome(displays =>
        {
            var displayArray = new JArray();
            foreach (var display in displays)
            {
                displayArray.Add(display.EncodeToJson());
            }
            result.Add(DisplayJsonKey, displayArray);
        });
        
        return result;
    }
}
