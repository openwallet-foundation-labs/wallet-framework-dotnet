using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;
using static WalletFramework.SdJwtVc.Models.VctMetadata.Claims.ClaimDisplayJsonKeys;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Claims;

/// <summary>
///     Represents the information about the logo to be displayed for the type.
/// </summary>
public readonly struct ClaimDisplay
{
    /// <summary>
    ///     Gets or sets the human-readable label for the claim.
    /// </summary>
    public Option<string> Label { get; }
    
    /// <summary>
    ///     Gets or sets the human-readable description for the claim.
    /// </summary>
    public Option<string> Description { get; }
    
    private ClaimDisplay(
        Option<string> label,
        Option<string> description)
    {
        Label = label;
        Description = description;
    }
        
    private static ClaimDisplay Create(
        Option<string> label,
        Option<string> description
    ) => new(
        label,
        description);
        
    public static Validation<ClaimDisplay> ValidClaimDisplay(JToken json)
    {
        var label = json.GetByKey(LabelJsonName).OnSuccess(token => token.ToJValue()).OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new StringIsNullOrWhitespaceError<SimpleRenderingMethod>();
                }

                return Valid(str);
            })
            .ToOption();
        
        var description = json.GetByKey(DescriptionJsonName).OnSuccess(token => token.ToJValue()).OnSuccess(value =>
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new StringIsNullOrWhitespaceError<SimpleRenderingMethod>();
                }

                return Valid(str);
            })
            .ToOption();

        return Valid(Create)
            .Apply(label)
            .Apply(description);
    }
}

public static class ClaimDisplayJsonKeys
{
    public const string LabelJsonName = "label";
    public const string DescriptionJsonName = "description";
}
