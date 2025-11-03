using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialDisplay;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.CredentialMetadataFun;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

/// <summary>
///     Represents credential metadata as defined in OpenID4VCI 1.0, containing display information and claims.
/// </summary>
public record CredentialMetadata
{
    /// <summary>
    ///     Gets a list of display properties of the supported credential for different languages.
    /// </summary>
    public Option<List<CredentialDisplay>> Display { get; }

    /// <summary>
    ///     Gets the claims token as provided by the issuer metadata. The concrete structure is format-specific.
    /// </summary>
    public Option<List<ClaimMetadata>> Claims { get; }

    private CredentialMetadata(Option<List<CredentialDisplay>> display, Option<List<ClaimMetadata>> claimMetadata)
    {
        Display = display;
        Claims = claimMetadata;
    }

    private static CredentialMetadata Create(
        Option<List<CredentialDisplay>> display, 
        Option<List<ClaimMetadata>> claimMetadata) => new(
        display,
        claimMetadata);
    
    public static Option<CredentialMetadata> OptionalCredentialMetadata(JToken token, Func<JToken, Validation<ClaimPath>> claimPathValidation) => token
        .ToJObject()
        .ToOption()
        .OnSome(jObject =>
        {
            var optionalCredentialDisplays = new Func<JToken, Option<List<CredentialDisplay>>>(credentialDisplays =>
                from array in credentialDisplays.ToJArray().ToOption()
                from displays in array.TraverseAny(OptionalCredentialDisplay)
                select displays.ToList());

            var optionalClaimsMetadata = new Func<JToken, Option<List<ClaimMetadata>>>(claimsMetadata =>
                from array in claimsMetadata.ToJArray().ToOption()
                from claimMetadatas in array.TraverseAny(claimMetadata => ClaimMetadata.ValidClaimMetadata(claimMetadata, claimPathValidation).ToOption())
                select claimMetadatas.ToList());

            return Valid(Create)
                .Apply(jObject.GetByKey(DisplayJsonKey).ToOption().OnSome(optionalCredentialDisplays))
                .Apply(jObject.GetByKey(ClaimsJsonKey).ToOption().OnSome(optionalClaimsMetadata)).ToOption();
        });
}

public static class CredentialMetadataFun
{
    public const string DisplayJsonKey = "display";
    public const string ClaimsJsonKey = "claims";

    public static JObject EncodeToJson(this CredentialMetadata metadata)
    {
        var result = new JObject();

        metadata.Display.IfSome(displays =>
        {
            var displayArray = new JArray();
            foreach (var display in displays)
            {
                displayArray.Add(display.EncodeToJson());
            }
            result.Add(DisplayJsonKey, displayArray);
        });

        metadata.Claims.IfSome(claims =>
        {
            var claimsArray = new JArray();
            foreach (var claim in claims)
            {
                claimsArray.Add(claim.EncodeToJson());
            }
            result.Add(ClaimsJsonKey, claimsArray);
        });

        return result;
    }
}
