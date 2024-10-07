using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models.VctMetadata.Claims;
using WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;
using static WalletFramework.Core.Functional.ValidationFun;
using static WalletFramework.SdJwtVc.Models.VctMetadata.VctMetadataClaimsJsonKeys;

namespace WalletFramework.SdJwtVc.Models.VctMetadata;

/// <summary>
///     Represents the claim information for the vc type.
/// </summary>
public readonly struct VctMetadataClaims
{
    /// <summary>
    ///     Gets or sets the claim or claims that are being addressed.
    /// </summary>
    public string[] Path { get; }
        
    /// <summary>
    ///     Gets or sets the display information for the claim.
    /// </summary>
    public Option<ClaimDisplay> Display { get; }
        
    /// <summary>
    ///     Gets or sets the infomration how to verify the claim.
    /// </summary>
    public Option<ClaimVerification> Verification { get; }
    
    /// <summary>
    ///     Gets or sets whether the claim is selectively discolsable.
    /// </summary>
    public Option<ClaimSelectiveDisclosure> SelectiveDisclosure { get; }
        
    private VctMetadataClaims(
        string[] path,
        Option<ClaimDisplay> display,
        Option<ClaimVerification> verification,
        Option<ClaimSelectiveDisclosure> selectiveDisclosure)
    {
        Path = path;
        Display = display;
        Verification = verification;
        SelectiveDisclosure = selectiveDisclosure;
    }
        
    private static VctMetadataClaims Create(
        string[] path,
        Option<ClaimDisplay> display,
        Option<ClaimVerification> verification,
        Option<ClaimSelectiveDisclosure> selectiveDisclosure
    ) => new(
        path,
        display,
        verification,
        selectiveDisclosure);
        
    public static Validation<VctMetadataClaims> ValidVctMetadataClaims(JToken json)
    {
        var path = json
            .GetByKey(PathJsonName)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(arr =>
            {
                return arr.Select(token => token.ToString()).ToArray();
            });
        
        var display = json
            .GetByKey(DisplayJsonName)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(ClaimDisplay.ValidClaimDisplay)
            .ToOption();
        
        var verification = json.GetByKey(VerificationJsonName).OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (!Enum.TryParse(str, true, out ClaimVerification verification))
                {
                    return new EnumCanNotBeParsedError<ClaimVerification>(str);
                }

                return Valid(verification);
            })
            .ToOption();
        
        var selectiveDisclosure = json.GetByKey(SelectiveDisclosureJsonName).OnSuccess(token => token.ToJValue())
            .OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (!Enum.TryParse(str, true, out ClaimSelectiveDisclosure selectiveDisclosure))
                {
                    return new EnumCanNotBeParsedError<ClaimSelectiveDisclosure>(str);
                }

                return Valid(selectiveDisclosure);
            })
            .ToOption();
        
        return Valid(Create)
            .Apply(path)
            .Apply(display)
            .Apply(verification)
            .Apply(selectiveDisclosure);
    }
}

public static class VctMetadataClaimsJsonKeys
{
    public const string PathJsonName = "path";
    public const string DisplayJsonName = "display";
    public const string VerificationJsonName = "verification";
    public const string SelectiveDisclosureJsonName = "sd";
}
