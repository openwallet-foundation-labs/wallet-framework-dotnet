using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Integrity;
using WalletFramework.Core.Json;
using static WalletFramework.SdJwtVc.Models.VctMetadata.Rendering.LogoJsonKeys;
using static WalletFramework.Core.Functional.ValidationFun;

namespace WalletFramework.SdJwtVc.Models.VctMetadata.Rendering;

/// <summary>
///     Represents the information about the logo to be displayed for the type.
/// </summary>
public readonly struct Logo
{
    /// <summary>
    ///     Gets or sets the URI pointing to the logo image.
    /// </summary>
    public Option<IntegrityUri> Uri { get; }
    
    /// <summary>
    ///     Gets or sets the alternative text for the logo image.
    /// </summary>
    public Option<string> AltText { get; }
    
    private Logo(
        Option<IntegrityUri> uri,
        Option<string> altText)
    {
        Uri = uri;
        AltText = altText;
    }
        
    private static Logo Create(
        Option<IntegrityUri> uri,
        Option<string> altText
    ) => new(
        uri,
        altText);
        
    public static Validation<Logo> ValidLogo(JObject json)
    {
        var uri = json
            .GetByKey(UriJsonName)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(extendsValue =>
            {
                var integrity = json.GetByKey(UriIntegrityJsonName)
                    .OnSuccess(token => token.ToJValue())
                    .OnSuccess(value => value.ToString(CultureInfo.InvariantCulture))
                    .ToOption();
                return IntegrityUri.ValidIntegrityUri(extendsValue.ToString(CultureInfo.InvariantCulture), integrity);
            })
            .ToOption();
        
        var altText = json.GetByKey(AltTextJsonName).OnSuccess(token => token.ToJValue()).OnSuccess(value =>
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
            .Apply(uri)
            .Apply(altText);
    }
}

public static class LogoJsonKeys
{
    public const string UriJsonName = "uri";
    public const string UriIntegrityJsonName = "uri#integrity";
    public const string AltTextJsonName = "alt_text";
}
