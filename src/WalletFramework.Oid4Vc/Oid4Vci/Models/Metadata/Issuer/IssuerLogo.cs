using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Uri;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.IssuerLogoFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;

/// <summary>
///     Represents the Logo of the Issuer.
/// </summary>
public record IssuerLogo
{
    /// <summary>
    ///     Gets the alternate text that describes the logo image. This is typically used for accessibility purposes.
    /// </summary>
    public Option<string> AltText { get; }

    /// <summary>
    ///     Gets the URL of the logo image.
    /// </summary>
    public Option<Uri> Uri { get; }
    
    private IssuerLogo(
        Option<string> altText,
        Option<Uri> uri)
    {
        AltText = altText;
        Uri = uri;
    }

    public static Option<IssuerLogo> OptionalIssuerLogo(JToken logo) => logo.ToJObject().ToOption().OnSome(jObject =>
    {
        var altText = jObject.GetByKey(AltTextJsonKey).ToOption().OnSome(text =>
        {
            var str = text.ToString();
            if (string.IsNullOrWhiteSpace(str))
                return Option<string>.None;

            return str;
        });
        
        var imageUri = jObject.GetByKey(UriJsonKey).ToOption().OnSome(uri =>
        {
            try
            {
                var str = uri.ToString();
                var result = new Uri(str);
                return result;
            }
            catch (Exception)
            {
                return Option<Uri>.None;
            }
        });

        if (altText.IsNone && imageUri.IsNone)
            return Option<IssuerLogo>.None;
            
        return new IssuerLogo(altText, imageUri);
    });
}

public static class IssuerLogoFun
{
    public const string AltTextJsonKey = "alt_text";
    public const string UriJsonKey = "uri";
    
    public static JObject EncodeToJson(this IssuerLogo logo)
    {
        var json = new JObject();
        logo.AltText.IfSome(altText => json.Add(AltTextJsonKey, altText));
        logo.Uri.IfSome(uri => json.Add(UriJsonKey, uri.ToStringWithoutTrail()));
        return json;
    }
}
